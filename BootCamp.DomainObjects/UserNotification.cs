using BootCamp.ObjectInterfaces;
using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects
{
    [Table("bc_user_notification")]
    public class UserNotification: IObjectState
    {
        [Key]
        public long  Id { get; set; }

        public NotificationType NotificationType { get; set; }

        public int? NotificationForId { get; set; }
        public int? LogId { get; set; } 
        public int SentByUserId { get; set; }

        public int ForUserId { get; set; }

        public bool IsDeleted { get; set; }
        [ForeignKey("NotificationForId")]
        public BootCamp.BootCamp BootCamp { get; set; }
        public bool? PushSent { get; set; }
        public string Title { get; set; }
        public string TextLine1 { get; set; }
        public string TextLine2 { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        [NotMapped]
        public ObjectState State { get; set; }
    }
}
