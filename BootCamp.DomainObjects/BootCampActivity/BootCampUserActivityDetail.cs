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
    [Table("bc_bootcamp_activitydetail")]
    public class BootCampUserActivityDetail : IObjectState
    {
        [Key]
        public long Id { get; set; }
        public ActivtyType ActivityType { get; set; }
        public TimeSpan StartTimeofDay { get; set; }
        public TimeSpan EndTimeofDay { get; set; }
        public bool? Missed { get; set; }
        public string ActivityName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public long BootCampUserActivityId { get; set; }
        [ForeignKey("BootCampUserActivityId")]
        public BootCampUserActivity BootCampUserActivity { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;set;
        }
    }

    public enum ActivtyType
    {
        Exercise = 0,
        Meal = 1
    }
}
