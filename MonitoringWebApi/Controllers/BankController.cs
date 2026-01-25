using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MonitoringWebApi.Services;

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
        await _bankConnectionService.ShutdownServerAsync();
    }

    [HttpGet("/log", Name = "Log")]
    public async Task Log()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var initMessage = Encoding.UTF8.GetBytes("Connected!\n");
            await webSocket.SendAsync(new ArraySegment<byte>(initMessage), WebSocketMessageType.Text, true, CancellationToken.None);

            var reader = await _bankConnectionService.GetLogStreamReader();
            
            try
            {
                while (reader.BaseStream.CanRead && webSocket.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    string? line;
                    try
                    {
                        line = await reader.ReadLineAsync(_cts.Token);
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while streaming logs");
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
