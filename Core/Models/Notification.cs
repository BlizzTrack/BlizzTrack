using System.Collections.Generic;

namespace Core.Models
{
    public enum NotificationType
    {
        Versions,
        PatchNotes,
        Ping
    }
    
    public class Notification
    {
        public NotificationType NotificationType { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}