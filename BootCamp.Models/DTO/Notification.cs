using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class Notification
    {
        public long Id { get; set; }
        public int BootCampId { get; set; }
        public int ActivityLogId { get; set; }
        public string Title { get; set; }
        public string TextLine1 { get; set; }
        public string TextLine2 { get; set; }
        public string NotificationTime { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsStarted { get; set; }
        public DateTime NotificationDateTime { get; set; }
        public string Image { get; set; }
    }
}
