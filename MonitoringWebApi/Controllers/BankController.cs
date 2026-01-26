using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
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
                reader = await _bankConnectionService.GetLogStreamReader();
                while (reader.CanRead && webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    string? line;
                    try
                    {
                        line = await reader.GetStream().ReadLineAsync(_cts.Token);
                        _logger.LogInformation($"Received log: {line}");
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    if (line == null)
                    {
                        break;
                    }

                    var buffer = Encoding.UTF8.GetBytes(line + "\n");
                    var segment = new ArraySegment<byte>(buffer);
                    if (webSocket.State != WebSocketState.Open)
                    {
                        break;
                    }
                    await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (SocketException)
            {
                _logger.LogError("Unable to connect to bank server for log streaming");
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
