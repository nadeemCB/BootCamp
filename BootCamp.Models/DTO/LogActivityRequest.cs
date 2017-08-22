using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class LogActivityRequest
    {
        public int Id { get; set; }
        public int BootCampId { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public LogPrivacyStatus LogStatus { get; set; }
        public ActivityStatus ActivityStatus { get; set; }
        public LogType LogType { get; set; }
        public int Duration { get; set; }
        public int WorkoutId { get; set; }
        public WorkoutLevel WorkoutLevel { get; set; }
    }
    
}
