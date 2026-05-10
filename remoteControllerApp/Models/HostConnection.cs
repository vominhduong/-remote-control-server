namespace remoteControllerApp.Models;

public class HostConnection
{
    public string HostId { get; set; } = string.Empty;

    public string ConnectionId { get; set; } = string.Empty;

    public string ComputerName { get; set; } = string.Empty;

    public DateTime ConnectedAt { get; set; }

    public DateTime LastSeenAt { get; set; }

    public bool IsOnline { get; set; }
}