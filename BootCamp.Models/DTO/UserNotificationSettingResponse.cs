using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class UserNotificationSettingResponse
    {
        public bool BootCampInvitations { get; set; }
        public bool MealReminder { get; set; }
        public bool WorkoutReminder { get; set; }
        public bool IsPublicProfile { get; set; }
        public TimeSpan BreakfastTime { get; set; }
        public TimeSpan FirstSnackTime { get; set; }
        public TimeSpan LunchTime { get; set; }
        public TimeSpan SecondSnackTime { get; set; }
        public TimeSpan DinnerTime { get; set; }
        public TimeSpan WorkoutTime { get; set; }
    }
}
