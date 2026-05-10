using Microsoft.AspNetCore.SignalR;
using remoteControllerApp.DTOs;
using remoteControllerApp.Manager;
using remoteControllerApp.Services;

namespace remoteControllerApp.Hubs;

public class RemoteHub : Hub
{
    private readonly ConnectionManager _connectionManager;
    private readonly SessionManager _sessionManager;
    private readonly SessionService _sessionService;

    public RemoteHub(
        ConnectionManager connectionManager,
        SessionManager sessionManager,
        SessionService sessionService)
    {
        _connectionManager = connectionManager;
        _sessionManager = sessionManager;
        _sessionService = sessionService;
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectionManager.MarkDisconnected(Context.ConnectionId);
        _sessionManager.EndSessionsByConnectionId(Context.ConnectionId);

        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");

        if (exception != null)
        {
            Console.WriteLine($"Disconnect error: {exception.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task RegisterHost(HostRegisterDto request)
    {
        if (string.IsNullOrWhiteSpace(request.HostId))
        {
            await Clients.Caller.SendAsync("RegisterHostFailed", "HostId is required");
            return;
        }

        _connectionManager.AddOrUpdateHost(
            request.HostId,
            Context.ConnectionId,
            request.ComputerName
        );

        await Groups.AddToGroupAsync(Context.ConnectionId, $"HOST_{request.HostId}");

        Console.WriteLine($"Host registered: {request.HostId} - {request.ComputerName}");

        await Clients.Caller.SendAsync("RegisterHostSuccess", request.HostId);
    }

    public async Task RegisterViewer(ViewerRegisterDto request)
    {
        if (string.IsNullOrWhiteSpace(request.ViewerId))
        {
            await Clients.Caller.SendAsync("RegisterViewerFailed", "ViewerId is required");
            return;
        }

        _connectionManager.AddOrUpdateViewer(
            request.ViewerId,
            Context.ConnectionId,
            request.ViewerName
        );

        await Groups.AddToGroupAsync(Context.ConnectionId, $"VIEWER_{request.ViewerId}");

        Console.WriteLine($"Viewer registered: {request.ViewerId} - {request.ViewerName}");

        await Clients.Caller.SendAsync("RegisterViewerSuccess", request.ViewerId);
    }

    public async Task PingHost(string hostId)
    {
        _connectionManager.UpdateHostLastSeen(hostId);

        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }

    public async Task PingViewer(string viewerId)
    {
        _connectionManager.UpdateViewerLastSeen(viewerId);

        await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }

    public async Task RequestControl(ControlRequestDto request)
    {
        try
        {
            var session = _sessionService.CreateControlRequest(request);

            Console.WriteLine($"Control request created. SessionId: {session.SessionId}, Viewer: {session.ViewerId}, Host: {session.HostId}");

            await Clients.Client(session.HostConnectionId).SendAsync("ReceiveControlRequest", new
            {
                session.SessionId,
                session.HostId,
                session.ViewerId,
                request.ViewerName,
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

        Console.WriteLine($"Session accepted: {response.SessionId}");

        await Clients.Client(session.ViewerConnectionId).SendAsync("ControlAccepted", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,
            Status = "Accepted",
            AcceptedAt = DateTime.UtcNow
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

        Console.WriteLine($"Session rejected: {response.SessionId}. Reason: {response.Reason}");

        await Clients.Client(session.ViewerConnectionId).SendAsync("ControlRejected", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,
            Status = "Rejected",
            Reason = response.Reason,
            RejectedAt = DateTime.UtcNow
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

        Console.WriteLine($"Session ended: {sessionId}");

        await Clients.Client(session.HostConnectionId).SendAsync("ControlEnded", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,
            Status = "Ended",
            EndedAt = DateTime.UtcNow
        });

        await Clients.Client(session.ViewerConnectionId).SendAsync("ControlEnded", new
        {
            session.SessionId,
            session.HostId,
            session.ViewerId,
            Status = "Ended",
            EndedAt = DateTime.UtcNow
        });
    }
}