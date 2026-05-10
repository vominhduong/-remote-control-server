namespace remoteControllerApp.DTOs;

public class ControlResponseDto
{
    public string SessionId { get; set; } = string.Empty;

    public bool Accepted { get; set; }

    public string? Reason { get; set; }
}