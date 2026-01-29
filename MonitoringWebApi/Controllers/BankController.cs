using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using Compression;
using Microsoft.AspNetCore.Mvc;
using MonitoringWebApi.Stream;
using MonitoringWebApi.Services.BankConnection;
using MonitoringWebApi.Services.KeyProvider;

namespace MonitoringWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BankController : ControllerBase
{
    private readonly ILogger<BankController> _logger;
    private readonly IBankConnectionService _bankConnectionService;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly byte[] _key;

    private readonly CancellationTokenSource _cts = new();

    public BankController(ILogger<BankController> logger, IBankConnectionService bankConnectionService, IHostApplicationLifetime appLifetime, IKeyProviderService keyProviderService)
    {
        _logger = logger;
        _bankConnectionService = bankConnectionService;
        _appLifetime = appLifetime;

        var privateKey = "";
        if (keyProviderService != null) privateKey = keyProviderService.GetKey();
        else logger.LogWarning("KeyProviderService not registered! Key is empty!");
        using (var sha256 = SHA256.Create())
        {
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(privateKey));
        }

        _appLifetime.ApplicationStopping.Register(() =>
        {
            _cts.Cancel();
        });
    }

    [HttpGet("/shutdown", Name = "Shutdown")]
    public async Task Shutdown()
    {
        _logger.LogInformation("Shutdown requested via API");
        await _bankConnectionService.ShutdownServerAsync();
        _logger.LogInformation("Shutdown request forwarded to bank server");
    }

    [HttpGet("/log", Name = "Log")]
    public async Task Log()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var initMessage = Encoding.UTF8.GetBytes("Connected!\n");
            await webSocket.SendAsync(new ArraySegment<byte>(initMessage), WebSocketMessageType.Text, true, CancellationToken.None);

            var lastHeartbeat = DateTime.UtcNow;
            using var heartbeatCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, heartbeatCts.Token);

            var receiveTask = Task.Run(async () =>
            {
                var buffer = new byte[1024];
                try
                {
                    while (webSocket.State == WebSocketState.Open && !linkedCts.Token.IsCancellationRequested)
                    {
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), linkedCts.Token);
                        if (result.MessageType == WebSocketMessageType.Close) break;

                        var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        if (msg.Trim() == "HEARTBEAT")
                        {
                            lastHeartbeat = DateTime.UtcNow;
                        }
                    }
                }
                catch
                {
                }
            });

            var monitorTask = Task.Run(async () =>
            {
                try
                {
                    while (!linkedCts.Token.IsCancellationRequested)
                    {
                        if (DateTime.UtcNow - lastHeartbeat > TimeSpan.FromSeconds(10))
                        {
                            _logger.LogWarning("Client heartbeat timeout. Closing connection.");
                            heartbeatCts.Cancel();
                            if (webSocket.State == WebSocketState.Open)
                            {
                                await webSocket.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Heartbeat timeout", CancellationToken.None);
                            }
                            break;
                        }
                        await Task.Delay(1000, linkedCts.Token);
                    }
                }
                catch
                {
                }
            });

            TcpClientStream? reader = null;
            try
            {
                while (webSocket.State == WebSocketState.Open && !linkedCts.Token.IsCancellationRequested)
                {
                    while (reader == null && webSocket.State == WebSocketState.Open && !linkedCts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            reader = await _bankConnectionService.GetLogStreamReader();
                        }
                        catch (SocketException)
                        {
                            if (webSocket.State == WebSocketState.Open)
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("CONNECTION_CLOSED" + "\n")), WebSocketMessageType.Text, true, CancellationToken.None);
                            _logger.LogWarning("Bank server not available, retrying in 3s...");
                            await Task.Delay(3000, linkedCts.Token);
                        }
                    }

                    // let the client know hes connected
                    if (webSocket.State == WebSocketState.Open && !linkedCts.Token.IsCancellationRequested)
                        await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("CONNECTION_ESTABLISHED" + "\n")), WebSocketMessageType.Text, true, CancellationToken.None);

                    // stream logs until connection drops
                    while (reader != null && reader.CanRead && webSocket.State == WebSocketState.Open && !linkedCts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var rawLine = await reader.GetStream().ReadLineAsync(linkedCts.Token);
                            if (rawLine == null) break;

                            var encryptedBytes = Convert.FromBase64String(rawLine);
                            
                            // Decrypt
                            byte[] decryptedBytes;
                            using (var aes = Aes.Create())
                            {
                                aes.Key = _key;
                                var iv = new byte[16];
                                Array.Copy(encryptedBytes, 0, iv, 0, 16);
                                aes.IV = iv;

                                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                                using (var msDecrypt = new MemoryStream(encryptedBytes, 16, encryptedBytes.Length - 16))
                                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                                using (var msPlain = new MemoryStream())
                                {
                                    csDecrypt.CopyTo(msPlain);
                                    decryptedBytes = msPlain.ToArray();
                                }
                            }

                            var decompressedBytes = new ZstdCompressor().Decompress(decryptedBytes);
                            var line = Encoding.UTF8.GetString(decompressedBytes);
                            _logger.LogInformation($"Received log: {line}");

                            var buffer = Encoding.UTF8.GetBytes(line + "\n");
                            if (webSocket.State != WebSocketState.Open) break;
                            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (IOException)
                        {
                            _logger.LogWarning("Lost connection to bank server");
                            if (webSocket.State == WebSocketState.Open)
                                await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("CONNECTION_CLOSED" + "\n")), WebSocketMessageType.Text, true, CancellationToken.None);
                            break;
                        }
                    }

                    // bank dropped, try reconnecting
                    reader?.Dispose();
                    reader = null;
                }
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while streaming logs");
            }
            finally
            {
                heartbeatCts.Cancel();
                reader?.Dispose();
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
