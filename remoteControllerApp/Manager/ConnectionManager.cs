using System.Collections.Concurrent;
using remoteControllerApp.Models;

namespace remoteControllerApp.Manager;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, HostConnection> _hosts = new();
    private readonly ConcurrentDictionary<string, ViewerConnection> _viewers = new();

    public void AddOrUpdateHost(string hostId, string connectionId, string computerName)
    {
        var now = DateTime.UtcNow;

        var host = new HostConnection
        {
            HostId = hostId,
            ConnectionId = connectionId,
            ComputerName = computerName,
            ConnectedAt = now,
            LastSeenAt = now,
            IsOnline = true
        };

        _hosts.AddOrUpdate(hostId, host, (_, existingHost) =>
        {
            existingHost.ConnectionId = connectionId;
            existingHost.ComputerName = computerName;
            existingHost.LastSeenAt = now;
            existingHost.IsOnline = true;

            return existingHost;
        });
    }

    public void AddOrUpdateViewer(string viewerId, string connectionId, string viewerName)
    {
        var now = DateTime.UtcNow;

        var viewer = new ViewerConnection
        {
            ViewerId = viewerId,
            ConnectionId = connectionId,
            ViewerName = viewerName,
            ConnectedAt = now,
            LastSeenAt = now,
            IsOnline = true
        };

        _viewers.AddOrUpdate(viewerId, viewer, (_, existingViewer) =>
        {
            existingViewer.ConnectionId = connectionId;
            existingViewer.ViewerName = viewerName;
            existingViewer.LastSeenAt = now;
            existingViewer.IsOnline = true;

            return existingViewer;
        });
    }

    public HostConnection? GetHostById(string hostId)
    {
        _hosts.TryGetValue(hostId, out var host);
        return host;
    }

    public ViewerConnection? GetViewerById(string viewerId)
    {
        _viewers.TryGetValue(viewerId, out var viewer);
        return viewer;
    }

    public HostConnection? GetHostByConnectionId(string connectionId)
    {
        return _hosts.Values.FirstOrDefault(x => x.ConnectionId == connectionId);
    }

    public ViewerConnection? GetViewerByConnectionId(string connectionId)
    {
        return _viewers.Values.FirstOrDefault(x => x.ConnectionId == connectionId);
    }

    public List<HostConnection> GetOnlineHosts()
    {
        return _hosts.Values
            .Where(x => x.IsOnline)
            .OrderByDescending(x => x.LastSeenAt)
            .ToList();
    }

    public List<ViewerConnection> GetOnlineViewers()
    {
        return _viewers.Values
            .Where(x => x.IsOnline)
            .OrderByDescending(x => x.LastSeenAt)
            .ToList();
    }

    public void MarkDisconnected(string connectionId)
    {
        var host = GetHostByConnectionId(connectionId);

        if (host != null)
        {
            host.IsOnline = false;
            host.LastSeenAt = DateTime.UtcNow;
            return;
        }

        var viewer = GetViewerByConnectionId(connectionId);

        if (viewer != null)
        {
            viewer.IsOnline = false;
            viewer.LastSeenAt = DateTime.UtcNow;
        }
    }

    public void UpdateHostLastSeen(string hostId)
    {
        if (_hosts.TryGetValue(hostId, out var host))
        {
            host.LastSeenAt = DateTime.UtcNow;
            host.IsOnline = true;
        }
    }

    public void UpdateViewerLastSeen(string viewerId)
    {
        if (_viewers.TryGetValue(viewerId, out var viewer))
        {
            viewer.LastSeenAt = DateTime.UtcNow;
            viewer.IsOnline = true;
        }
    }
}