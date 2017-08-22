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
    [Table("bc_bootcamp_weekly_checkin")]
    public class WeeklyCheckIn : IObjectState
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public BootCampUserDetail BootCampUserDetail { get; set; }
        public bool Measurements { get; set; }
        public bool Weight { get; set; }
        public bool BodyImages { get; set; }
        public bool Grocery { get; set; }
        public int WeekNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;
            set;
        }
    }
}
