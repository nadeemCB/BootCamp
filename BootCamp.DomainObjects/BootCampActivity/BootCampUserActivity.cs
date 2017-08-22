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
    [Table("bc_bootcamp_useractivity")]
    public class BootCampUserActivity: IObjectState
    {
        [Key]
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public DayOfWeek DayofWeek { get; set; }
        public int WeekNo { get; set; }
        public int BootCampUserId { get; set; }
        [ForeignKey("BootCampUserId")]
        public BootCampUserDetail BootCampUserDetail { get; set; }
        public int DayNo { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<BootCampUserActivityDetail> BootCampUserActivityDetails { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;
            set;
        }
    }
}
