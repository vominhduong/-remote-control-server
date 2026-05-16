using System;
using System.Collections.Generic;
using System.Text;

namespace remoteControllerApp.DTOs
{
    public class ControlAcceptedDto
    {
        public string SessionId { get; set; } = "";
        public string HostId { get; set; } = "";
        public string ViewerId { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime AcceptedAt { get; set; }
    }
}
