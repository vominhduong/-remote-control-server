using Microsoft.AspNetCore.Mvc;
using remoteControllerApp.Services;

namespace remoteControllerApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly SessionService _sessionService;

    public SessionController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public IActionResult GetAllSessions()
    {
        var sessions = _sessionService.GetAllSessions();

        return Ok(sessions);
    }

    [HttpGet("active")]
    public IActionResult GetActiveSessions()
    {
        var sessions = _sessionService.GetActiveSessions();

        return Ok(sessions);
    }

    [HttpGet("{sessionId}")]
    public IActionResult GetSessionById(string sessionId)
    {
        var session = _sessionService.GetSessionById(sessionId);

        if (session == null)
        {
            return NotFound(new
            {
                message = "Session not found."
            });
        }

        return Ok(session);
    }
}