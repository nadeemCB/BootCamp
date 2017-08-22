using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects
{
    [Table("bc_bootcamp_user")]
    public class UserBootCamp
    {
        [Key]
        public int Id { get; set; }
        public int BootCampId { get; set; }
        
        [ForeignKey("BootCampId")]
        public BootCampSummary BootCamp { get; set; }
        public int UserId { get; set; }
        public bool DroppedOut { get; set; }

        [ForeignKey("UserId")]
        public User BootCampUser { get; set; }

        public DateTime JoinDate { get; set; }
    }
}
