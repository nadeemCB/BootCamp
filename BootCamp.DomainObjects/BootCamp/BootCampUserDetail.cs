using BootCamp.ObjectInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.BootCamp
{
    [Table("bc_bootcamp_user")]
    public class BootCampUserDetail: IObjectState
    {
        [Key]
        public int Id { get; set; }

        public int BootCampId { get; set; }
        [ForeignKey("BootCampId")]
        public BootCamp BootCamp { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public BootCampUser BootCampUser { get; set; }

        public DateTime JoinDate { get; set; }

        public bool DroppedOut { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public virtual IList<ActivityLog> ActivityLogs { get; set; }

        public virtual IList<WeeklyCheckIn> WeeklyCheckIns { get; set; }

        [NotMapped]
        public ObjectState State { get; set; }
    }
}
