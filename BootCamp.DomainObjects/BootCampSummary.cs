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
    [Table("bc_bootcamp")]
    public class BootCampSummary
    {
        [Key]
        public int Id { get; set; }
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public User Owner { get; set; }
        public BootCampStatus BootCampStatus { get; set; }
        public string Name { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public int MaxMembers { get; set; }
        public int SignedUp { get; set; }
        public string BootcampImage { get; set; }
    }
}
