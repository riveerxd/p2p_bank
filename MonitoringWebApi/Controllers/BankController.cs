using Microsoft.AspNetCore.Mvc;
using MonitoringWebApi.Services;

namespace MonitoringWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BankController : ControllerBase
{
    private readonly ILogger<BankController> _logger;
    private readonly IBankConnectionService _bankConnectionService;

    public BankController(ILogger<BankController> logger, IBankConnectionService bankConnectionService)
    {
        _logger = logger;
        _bankConnectionService = bankConnectionService;
    }

    [HttpGet("/shutdown", Name = "Shutdown")]
    public async Task Shutdown()
    {
        await _bankConnectionService.ShutdownServerAsync();
    }
}
