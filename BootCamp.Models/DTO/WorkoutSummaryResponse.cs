using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class WorkoutSummaryResponse
    {
        public string BootCampName { get; set; }
        public string BootCampImage { get; set; }
        public string UserImage { get; set; }
        public int Id { get; set; }
        public DateTime CompletionDate { get; set; }
        public DateTime StartDate { get; set; }
        public WorkoutSummaryData Data { get; set; }
    }

    public class WorkoutSummaryData
    {
        public int Beginner { get; set; }
        public int Intermediate { get; set; }
        public int Expert { get; set; }
        public int HomeExercise { get; set; }
        public int GymExercise { get; set; }

        public List<string> CurrentWeekImages { get; set; }
        public List<string> PastImages { get; set; }
    }
}
