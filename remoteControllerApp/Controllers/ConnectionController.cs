using Microsoft.AspNetCore.Mvc;
using remoteControllerApp.Services;

namespace remoteControllerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConnectionController : ControllerBase
{
    private readonly ConnectionService _connectionService;

    public ConnectionController(ConnectionService connectionService)
    {
        _connectionService = connectionService;
    }

    [HttpGet("hosts")]
    public IActionResult GetOnlineHosts()
    {
        var hosts = _connectionService.GetOnlineHosts();

        return Ok(hosts);
    }

    [HttpGet("viewers")]
    public IActionResult GetOnlineViewers()
    {
        var viewers = _connectionService.GetOnlineViewers();

        return Ok(viewers);
    }
}