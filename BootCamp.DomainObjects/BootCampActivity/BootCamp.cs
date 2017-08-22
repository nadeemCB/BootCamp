using BootCamp.ObjectInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.BootCampActivity
{
    [Table("bc_bootcamp")]
    public class BootCamp : IObjectState
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string BootcampImage { get; set; }
        [Column("Private")]
        public bool IsPrivate { get; set; }
        public int CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public BootCampUser Owner { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public virtual IList<BootCampUserDetail> BootCampUserDetails { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;
            set;
        }
    }
}
