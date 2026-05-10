using remoteControllerApp.DTOs;
using remoteControllerApp.Manager;

namespace remoteControllerApp.Services;

public class ConnectionService
{
    private readonly ConnectionManager _connectionManager;

    public ConnectionService(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public List<ConnectionInfoDto> GetOnlineHosts()
    {
        return _connectionManager.GetOnlineHosts()
            .Select(host => new ConnectionInfoDto
            {
                Id = host.HostId,
                ConnectionId = host.ConnectionId,
                Name = host.ComputerName,
                IsOnline = host.IsOnline,
                ConnectedAt = host.ConnectedAt,
                LastSeenAt = host.LastSeenAt
            })
            .ToList();
    }

    public List<ConnectionInfoDto> GetOnlineViewers()
    {
        return _connectionManager.GetOnlineViewers()
            .Select(viewer => new ConnectionInfoDto
            {
                Id = viewer.ViewerId,
                ConnectionId = viewer.ConnectionId,
                Name = viewer.ViewerName,
                IsOnline = viewer.IsOnline,
                ConnectedAt = viewer.ConnectedAt,
                LastSeenAt = viewer.LastSeenAt
            })
            .ToList();
    }
}