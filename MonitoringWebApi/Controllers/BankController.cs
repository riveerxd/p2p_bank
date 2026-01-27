using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Compression;
using Microsoft.AspNetCore.Mvc;
using MonitoringWebApi.Services;
using MonitoringWebApi.Stream;

namespace MonitoringWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BankController : ControllerBase
{
    private readonly ILogger<BankController> _logger;
    private readonly IBankConnectionService _bankConnectionService;
    private readonly IHostApplicationLifetime _appLifetime;

    private readonly CancellationTokenSource _cts = new();

    public BankController(ILogger<BankController> logger, IBankConnectionService bankConnectionService, IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _bankConnectionService = bankConnectionService;
        _appLifetime = appLifetime;

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

            TcpClientStream? reader = null;
            try
            {
                while (webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    // retry until bank is up or client disconnects
                    while (reader == null && webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            reader = await _bankConnectionService.GetLogStreamReader();
                        }
                        catch (SocketException)
                        {
                            _logger.LogWarning("Bank server not available, retrying in 3s...");
                            await Task.Delay(3000, _cts.Token);
                        }
                    }

                    // stream logs until connection drops
                    while (reader != null && reader.CanRead && webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            var rawLine = await reader.GetStream().ReadLineAsync(_cts.Token);
                            if (rawLine == null) break;

                            var decodedBytes = Convert.FromBase64String(rawLine);
                            var decompressedBytes = new ZstdCompressor().Decompress(decodedBytes);
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
                reader?.Dispose();
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
