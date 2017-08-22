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
    [Table("bc_bootcamp_inviteduser")]
    public class BootCampInvitedUsers: IObjectState
    {
        [Key]
        public int Id { get; set; }
        public int BootCampId { get; set; }
        [ForeignKey("BootCampId")]
        public BootCamp BootCamp { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public BootCampUser User { get; set; }
        [NotMapped]
        public ObjectState State { get; set; }
    }
}
