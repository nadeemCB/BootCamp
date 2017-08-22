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
    [Table("bc_workout")]
    public class Workout: IObjectState
    {
        [Key]
        public int Id { get; set; }
        public int WeekNo { get; set; }
        public int WeekDayNo { get; set; }
        public WorkoutType WorkoutType { get; set; }
        public WorkoutLevel WorkoutLevel { get; set; }
        public string WorkoutName { get; set; }
        public string WorkoutDescription { get; set; }
        public Nullable<int> WarmupTime { get; set; }
        public string Description { get; set; }
        public virtual IList<Exercise> Exercises { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;set;
        }
    }
}
