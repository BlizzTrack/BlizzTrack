using System.Collections.Generic;

namespace Core.Models
{
    public enum NotificationType
    {
        Versions,
        PatchNotes
    }
    
    public class Notification
    {
        public NotificationType NotificationType { get; set; }
        public Dictionary<string, object> Payload { get; set; }
    }
}