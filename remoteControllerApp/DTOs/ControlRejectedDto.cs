using System;
using System.Collections.Generic;
using System.Text;

namespace remoteControllerApp.DTOs
{
    public class ControlRejectedDto
    {
        public string SessionId { get; set; } = "";
        public string HostId { get; set; } = "";
        public string ViewerId { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Reason { get; set; }
        public DateTime RejectedAt { get; set; }
    }
}
