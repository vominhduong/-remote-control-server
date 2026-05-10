namespace remoteControllerApp.DTOs;

public class ControlRequestDto
{
    public string HostId { get; set; } = string.Empty;

    public string ViewerId { get; set; } = string.Empty;

    public string ViewerName { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
}