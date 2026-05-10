namespace remoteControllerApp.DTOs;

public class UpdateUserDto
{
    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}