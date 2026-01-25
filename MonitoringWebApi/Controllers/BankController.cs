using Microsoft.AspNetCore.Mvc;

namespace MonitoringWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BankController : ControllerBase
{
    private readonly ILogger<BankController> _logger;

    public BankController(ILogger<BankController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/shutdown", Name = "Shutdown")]
    public async Task Shutdown()
    {
        
    }
}
