using remoteControllerApp.DTOs;
using remoteControllerApp.Manager;
using remoteControllerApp.Models;

namespace remoteControllerApp.Services;

public class SessionService
{
    private readonly ConnectionManager _connectionManager;
    private readonly SessionManager _sessionManager;

    public SessionService(
        ConnectionManager connectionManager,
        SessionManager sessionManager)
    {
        _connectionManager = connectionManager;
        _sessionManager = sessionManager;
    }

    public RemoteSession CreateControlRequest(ControlRequestDto request)
    {
        var host = _connectionManager.GetHostById(request.HostId);
        if (host == null || !host.IsOnline)
        {
            throw new InvalidOperationException("Host is not online.");
        }

        var viewer = _connectionManager.GetViewerById(request.ViewerId);
        if (viewer == null || !viewer.IsOnline)
        {
            throw new InvalidOperationException("Viewer is not online.");
        }

        var session = _sessionManager.CreateSession(
            request.HostId,
            request.ViewerId,
            host.ConnectionId,
            viewer.ConnectionId
        );

        return session;
    }

    public bool AcceptSession(string sessionId)
    {
        return _sessionManager.AcceptSession(sessionId);
    }

    public bool RejectSession(string sessionId, string? reason)
    {
        return _sessionManager.RejectSession(sessionId, reason);
    }

    public bool EndSession(string sessionId)
    {
        return _sessionManager.EndSession(sessionId);
    }

    public List<SessionInfoDto> GetAllSessions()
    {
        return _sessionManager.GetAllSessions()
            .Select(ToDto)
            .ToList();
    }

    public List<SessionInfoDto> GetActiveSessions()
    {
        return _sessionManager.GetActiveSessions()
            .Select(ToDto)
            .ToList();
    }

    public SessionInfoDto? GetSessionById(string sessionId)
    {
        var session = _sessionManager.GetSessionById(sessionId);
        return session == null ? null : ToDto(session);
    }

    private static SessionInfoDto ToDto(RemoteSession session)
    {
        return new SessionInfoDto
        {
            SessionId = session.SessionId,
            HostId = session.HostId,
            ViewerId = session.ViewerId,
            Status = session.Status,
            CreatedAt = session.CreatedAt,
            AcceptedAt = session.AcceptedAt,
            RejectedAt = session.RejectedAt,
            EndedAt = session.EndedAt,
            RejectReason = session.RejectReason
        };
    }
}