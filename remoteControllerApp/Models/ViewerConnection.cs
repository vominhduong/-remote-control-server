namespace remoteControllerApp.Models;

public class ViewerConnection
{
    public string ViewerId { get; set; } = string.Empty;

    public string ConnectionId { get; set; } = string.Empty;

    public string ViewerName { get; set; } = string.Empty;

    public DateTime ConnectedAt { get; set; }

    public DateTime LastSeenAt { get; set; }

    public bool IsOnline { get; set; }
}