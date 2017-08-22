using BootCamp.ObjectInterfaces;
using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.BootCamp
{
    [Table("bc_activitylog")]
    public class ActivityLog : IObjectState
    {
        [Key]
        public int Id { get; set; }
        public string Image { get; set; }
        public int BootCampUserId { get; set; }
        [ForeignKey("BootCampUserId")]
        public BootCampUserDetail BootCampUserDetail { get; set; }
        public string Description { get; set; }
        public LogPrivacyStatus LogPrivacyStatus { get; set; }
        public ActivityStatus ActivityStatus { get; set; }
        public LogType LogType { get; set; }
        public Nullable<int> Duration { get; set; }
        public TimeSpan ActivityEndTime { get; set; }
        public TimeSpan ActivityStartTime { get; set; }
        public DateTime LogTime { get; set; }
        public Nullable<MealType> MealType { get; set; }
        public Nullable<int> WorkoutId { get; set; }
        public Nullable<WorkoutLevel> WorkoutLevel { get; set; }
        public int DayNo { get; set; }
        public int WeekNo { get; set; }
        public int WeekDayNo { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;
            set;
        }
    }
}
