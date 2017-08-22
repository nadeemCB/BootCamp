using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class ExercisesByLevelResponse
    {
        public List<Models.Exercise.weeksWeekDayWorkouts> WorkOuts { get; set; }
    }
}
