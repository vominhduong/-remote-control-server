using Microsoft.AspNetCore.SignalR;
using remoteControllerApp.DTOs;
using remoteControllerApp.Manager;
using remoteControllerApp.Repositories;
using remoteControllerApp.Services;

namespace remoteControllerApp.Hubs;

public class RemoteHub : Hub
{
    private readonly ConnectionManager _connectionManager;
    private readonly SessionManager _sessionManager;
    private readonly SessionService _sessionService;
    private readonly IRealtimeDatabaseRepository _realtimeDatabase;

    public RemoteHub(
        ConnectionManager connectionManager,
        SessionManager sessionManager,
        SessionService sessionService,
        IRealtimeDatabaseRepository realtimeDatabase)
    {
        _connectionManager = connectionManager;
        _sessionManager = sessionManager;
        _sessionService = sessionService;
        _realtimeDatabase = realtimeDatabase;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var now = DateTime.UtcNow;

        var host = _connectionManager.GetHostByConnectionId(Context.ConnectionId);
        var viewer = _connectionManager.GetViewerByConnectionId(Context.ConnectionId);

        var endedSessions = _sessionManager.EndSessionsByConnectionId(Context.ConnectionId);

        _connectionManager.MarkDisconnected(Context.ConnectionId);

        if (host != null)
        {
            await _realtimeDatabase.PatchAsync($"hosts/{host.HostId}", new
            {
                isOnline = false,
                disconnectedAt = now,
                lastSeenAt = now,
                connectionId = ""
            });
        }

        if (viewer != null)
        {
            await _realtimeDatabase.PatchAsync($"viewers/{viewer.ViewerId}", new
            {
                isOnline = false,
                disconnectedAt = now,
                lastSeenAt = now,
                connectionId = ""
            });
        }

        foreach (var session in endedSessions)
        {
            await _realtimeDatabase.PatchAsync($"sessions/{session.SessionId}", new
            {
                status = "Ended",
                endedAt = session.EndedAt ?? now,
                endReason = "Client disconnected"
            });
        }

        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");

        if (exception != null)
        {
            Console.WriteLine($"Disconnect error: {exception.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RegisterHost(HostRegisterDto request)
    {
        if (request == null)
            throw new HubException("Request is null.");

        if (string.IsNullOrWhiteSpace(request.HostId))
            throw new HubException("HostId is required.");

        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new HubException("UserId is required.");

        var now = DateTime.UtcNow;

        try
        {
            _connectionManager.AddOrUpdateHost(
                request.HostId,
                Context.ConnectionId,
                request.ComputerName
            );

            await _realtimeDatabase.SetAsync($"hosts/{request.HostId}", new
            {
                hostId = request.HostId,
                computerName = request.ComputerName,
                ownerUserId = request.UserId,
                connectionId = Context.ConnectionId,
                isOnline = true,
                connectedAt = now,
                lastSeenAt = now
            });

            await _realtimeDatabase.SetAsync($"user_hosts/{request.UserId}/{request.HostId}", new
            {
                hostId = request.HostId,
                computerName = request.ComputerName,
                isOnline = true,
                lastSeenAt = now
            });

            await Groups.AddToGroupAsync(Context.ConnectionId, $"HOST_{request.HostId}");

            Console.WriteLine($"Host registered: {request.HostId} - {request.ComputerName}");

            await Clients.Caller.SendAsync("RegisterHostSuccess", request.HostId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RegisterHost error: {ex}");

            throw new HubException($"RegisterHost failed: {ex.Message}");
        }
    }

    public async Task RegisterViewer(ViewerRegisterDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ViewerId))
        {
            await Clients.Caller.SendAsync("RegisterViewerFailed", "ViewerId is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            await Clients.Caller.SendAsync("RegisterViewerFailed", "UserId is required.");
            return;
        }

        var now = DateTime.UtcNow;

        _connectionManager.AddOrUpdateViewer(
            request.ViewerId,
            Context.ConnectionId,
            request.ViewerName
        );

        await _realtimeDatabase.SetAsync($"viewers/{request.ViewerId}", new
        {
            viewerId = request.ViewerId,
            viewerName = request.ViewerName,
            userId = request.UserId,
            connectionId = Context.ConnectionId,
            isOnline = true,
            connectedAt = now,
            lastSeenAt = now
        });

        await _realtimeDatabase.SetAsync($"user_viewers/{request.UserId}/{request.ViewerId}", new
        {
            viewerId = request.ViewerId,
            viewerName = request.ViewerName,
            isOnline = true,
            lastSeenAt = now
        });

        await Groups.AddToGroupAsync(Context.ConnectionId, $"VIEWER_{request.ViewerId}");

        Console.WriteLine($"Viewer registered: {request.ViewerId} - {request.ViewerName}");

        await Clients.Caller.SendAsync("RegisterViewerSuccess", request.ViewerId);
    }

    public async Task PingHost(string hostId)
    {
        var now = DateTime.UtcNow;

        _connectionManager.UpdateHostLastSeen(hostId);

        await _realtimeDatabase.PatchAsync($"hosts/{hostId}", new
        {
            lastSeenAt = now,
            isOnline = true
        });

        await Clients.Caller.SendAsync("Pong", now);
    }

    public async Task PingViewer(string viewerId)
    {
        var now = DateTime.UtcNow;

        _connectionManager.UpdateViewerLastSeen(viewerId);

        await _realtimeDatabase.PatchAsync($"viewers/{viewerId}", new
        {
            lastSeenAt = now,
            isOnline = true
        });

        await Clients.Caller.SendAsync("Pong", now);
    }

    public async Task RequestControl(ControlRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
            {
                await Clients.Caller.SendAsync("ControlRequestFailed", "UserId is required.");
                return;
            }

            var session = _sessionService.CreateControlRequest(request);

            await _realtimeDatabase.SetAsync($"sessions/{session.SessionId}", new
            {
                sessionId = session.SessionId,
                hostId = session.HostId,
                viewerId = session.ViewerId,
                viewerUserId = request.UserId,
                hostConnectionId = session.HostConnectionId,
                viewerConnectionId = session.ViewerConnectionId,
                status = session.Status,
                createdAt = session.CreatedAt,
                acceptedAt = session.AcceptedAt,
                rejectedAt = session.RejectedAt,
                endedAt = session.EndedAt,
                rejectReason = session.RejectReason
            });

            await _realtimeDatabase.SetAsync($"user_sessions/{request.UserId}/{session.SessionId}", new
            {
                sessionId = session.SessionId,
                hostId = session.HostId,
                viewerId = session.ViewerId,
                status = session.Status,
                createdAt = session.CreatedAt
            });

            Console.WriteLine(
                $"Control request created. SessionId: {session.SessionId}, Viewer: {session.ViewerId}, Host: {session.HostId}"
            );

            await Clients.Client(session.HostConnectionId).SendAsync("ReceiveControlRequest", new
            {
                session.SessionId,
                session.HostId,
                session.ViewerId,
                request.ViewerName,
                request.UserId,
                session.CreatedAt
            });

            await Clients.Caller.SendAsync("ControlRequestSent", new
            {
                session.SessionId,
                session.HostId,
                session.ViewerId,
                session.Status,
                session.CreatedAt
            });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("ControlRequestFailed", ex.Message);
        }
    }

    public async Task AcceptControl(ControlResponseDto response)
    {
        var session = _sessionManager.GetSessionById(response.SessionId);

        if (session == null)
        {
            await Clients.Caller.SendAsync("AcceptControlFailed", "Session not found.");
            return;
        }

        var accepted = _sessionService.AcceptSession(response.SessionId);

        if (!accepted)
        {
            await Clients.Caller.SendAsync("AcceptControlFailed", "Cannot accept session.");
            return;
        }

        var acceptedAt = DateTime.UtcNow;

        await _realtimeDatabase.PatchAsync($"sessions/{response.SessionId}", new
        {
            status = "Accepted",
            acceptedAt
        });

        Console.WriteLine($"Session accepted: {response.SessionId}");

        await Clients.Client(session.ViewerConnectionId).SendAsync("ControlAccepted", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,
            Status = "Accepted",
            AcceptedAt = acceptedAt
        });

        await Clients.Caller.SendAsync("AcceptControlSuccess", response.SessionId);
    }

    public async Task RejectControl(ControlResponseDto response)
    {
        var session = _sessionManager.GetSessionById(response.SessionId);

        if (session == null)
        {
            await Clients.Caller.SendAsync("RejectControlFailed", "Session not found.");
            return;
        }

        var rejected = _sessionService.RejectSession(response.SessionId, response.Reason);

        if (!rejected)
        {
            await Clients.Caller.SendAsync("RejectControlFailed", "Cannot reject session.");
            return;
        }

        var rejectedAt = DateTime.UtcNow;

        await _realtimeDatabase.PatchAsync($"sessions/{response.SessionId}", new
        {
            status = "Rejected",
            rejectedAt,
            rejectReason = response.Reason
        });

        Console.WriteLine($"Session rejected: {response.SessionId}. Reason: {response.Reason}");

        await Clients.Client(session.ViewerConnectionId).SendAsync("ControlRejected", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,
            Status = "Rejected",
            Reason = response.Reason,
            RejectedAt = rejectedAt
        });

        await Clients.Caller.SendAsync("RejectControlSuccess", response.SessionId);
    }

    public async Task EndControl(string sessionId)
    {
        var session = _sessionManager.GetSessionById(sessionId);

        if (session == null)
        {
            await Clients.Caller.SendAsync("EndControlFailed", "Session not found.");
            return;
        }

        var ended = _sessionService.EndSession(sessionId);

        if (!ended)
        {
            await Clients.Caller.SendAsync("EndControlFailed", "Cannot end session.");
            return;
        }

        var endedAt = DateTime.UtcNow;

        await _realtimeDatabase.PatchAsync($"sessions/{sessionId}", new
        {
            status = "Ended",
            endedAt,
            endReason = "User ended control"
        });

        Console.WriteLine($"Session ended: {sessionId}");

        await Clients.Client(session.HostConnectionId).SendAsync("ControlEnded", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,
            Status = "Ended",
            EndedAt = endedAt
        });

        await Clients.Client(session.ViewerConnectionId).SendAsync("ControlEnded", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,
            Status = "Ended",
            EndedAt = endedAt
        });
    }

    public async Task SendScreenFrame(ScreenFrameDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            await Clients.Caller.SendAsync("SendScreenFrameFailed", "SessionId is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            await Clients.Caller.SendAsync("SendScreenFrameFailed", "ImageBase64 is required.");
            return;
        }

        var session = _sessionManager.GetAcceptedSession(request.SessionId);

        if (session == null)
        {
            await Clients.Caller.SendAsync("SendScreenFrameFailed", "Session is not accepted or not found.");
            return;
        }

        await Clients.Client(session.ViewerConnectionId).SendAsync("ReceiveScreenFrame", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,

            request.ImageBase64,

            request.ScreenWidth,
            request.ScreenHeight,
            request.FrameWidth,
            request.FrameHeight,

            request.MouseX,
            request.MouseY,

            request.SentAt,
            ReceivedAt = DateTime.UtcNow
        });
    }

    public async Task SendMouseEvent(MouseEventDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            await Clients.Caller.SendAsync("SendMouseEventFailed", "SessionId is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            await Clients.Caller.SendAsync("SendMouseEventFailed", "Mouse action is required.");
            return;
        }

        var session = _sessionManager.GetAcceptedSession(request.SessionId);

        if (session == null)
        {
            await Clients.Caller.SendAsync("SendMouseEventFailed", "Session is not accepted or not found.");
            return;
        }

        await Clients.Client(session.HostConnectionId).SendAsync("ReceiveMouseEvent", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,

            request.Action,
            request.X,
            request.Y,
            request.ScreenWidth,
            request.ScreenHeight,
            request.Button,
            request.Delta,

            request.SentAt,
            ReceivedAt = DateTime.UtcNow
        });
    }

    public async Task SendKeyboardEvent(KeyboardEventDto request)
    {
        if (string.IsNullOrWhiteSpace(request.SessionId))
        {
            await Clients.Caller.SendAsync("SendKeyboardEventFailed", "SessionId is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            await Clients.Caller.SendAsync("SendKeyboardEventFailed", "Keyboard action is required.");
            return;
        }

        var session = _sessionManager.GetAcceptedSession(request.SessionId);

        if (session == null)
        {
            await Clients.Caller.SendAsync("SendKeyboardEventFailed", "Session is not accepted or not found.");
            return;
        }

        await Clients.Client(session.HostConnectionId).SendAsync("ReceiveKeyboardEvent", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,

            request.Action,
            request.Key,
            request.Code,
            request.CtrlKey,
            request.ShiftKey,
            request.AltKey,

            request.SentAt,
            ReceivedAt = DateTime.UtcNow
        });
    }
}