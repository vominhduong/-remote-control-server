using System.Collections.Concurrent;
using remoteControllerApp.Models;

namespace remoteControllerApp.Manager;

public class SessionManager
{
    private readonly ConcurrentDictionary<string, RemoteSession> _sessions = new();

    public RemoteSession CreateSession(
        string hostId,
        string viewerId,
        string hostConnectionId,
        string viewerConnectionId)
    {
        var session = new RemoteSession
        {
            SessionId = Guid.NewGuid().ToString("N"),
            HostId = hostId,
            ViewerId = viewerId,
            HostConnectionId = hostConnectionId,
            ViewerConnectionId = viewerConnectionId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _sessions[session.SessionId] = session;

        return session;
    }

    public RemoteSession? GetSessionById(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    public List<RemoteSession> GetAllSessions()
    {
        return _sessions.Values
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public List<RemoteSession> GetActiveSessions()
    {
        return _sessions.Values
            .Where(x => x.Status == "Pending" || x.Status == "Accepted")
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    public bool AcceptSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }

        session.Status = "Accepted";
        session.AcceptedAt = DateTime.UtcNow;

        return true;
    }

    public bool RejectSession(string sessionId, string? reason)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }

        session.Status = "Rejected";
        session.RejectedAt = DateTime.UtcNow;
        session.RejectReason = reason;

        return true;
    }

    public bool EndSession(string sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            return false;
        }

        session.Status = "Ended";
        session.EndedAt = DateTime.UtcNow;

        return true;
    }

    public void EndSessionsByConnectionId(string connectionId)
    {
        var relatedSessions = _sessions.Values
            .Where(x =>
                x.Status is "Pending" or "Accepted" &&
                (x.HostConnectionId == connectionId || x.ViewerConnectionId == connectionId))
            .ToList();

        foreach (var session in relatedSessions)
        {
            session.Status = "Ended";
            session.EndedAt = DateTime.UtcNow;
        }
    }
}