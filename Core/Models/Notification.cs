using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public class NotificationHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Code { get; set; }
        
        public string Seqn { get; set; }
        
        public string File { get; set; }
        
        public NotificationType NotificationType { get; set; }
        
        public DateTime Sent { get; set; }
    }
}