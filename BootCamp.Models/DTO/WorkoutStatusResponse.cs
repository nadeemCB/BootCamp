using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class WorkoutStatusResponse
    {
        public ActivityStatus WorkoutActivityStatus { get; set; }
        public WorkoutType WorkoutType { get; set; }
        public Nullable<DateTime> LogTime { get; set; } 
    }
}
