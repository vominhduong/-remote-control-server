namespace remoteControllerApp.DTOs;

public class ConnectionInfoDto
{
    public string Id { get; set; } = string.Empty;

    public string ConnectionId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsOnline { get; set; }

    public DateTime ConnectedAt { get; set; }

    public DateTime LastSeenAt { get; set; }
}