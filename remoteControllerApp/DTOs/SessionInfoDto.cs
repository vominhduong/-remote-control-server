namespace remoteControllerApp.DTOs;

public class SessionInfoDto
{
    public string SessionId { get; set; } = string.Empty;

    public string HostId { get; set; } = string.Empty;

    public string ViewerId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public string? RejectReason { get; set; }
}