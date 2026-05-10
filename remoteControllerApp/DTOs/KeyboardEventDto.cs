public class KeyboardEventDto
{
    public string SessionId { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public bool CtrlKey { get; set; }

    public bool ShiftKey { get; set; }

    public bool AltKey { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}