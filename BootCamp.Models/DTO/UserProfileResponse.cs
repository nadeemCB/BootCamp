using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class UserProfileResponse
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ProfileImage { get; set; }

        public bool BootCampInvitations { get; set; }

        public bool MealReminder { get; set; }

        public bool WorkoutReminder { get; set; }

        public bool IsPublicProfile { get; set; }

        public bool PaidAccount { get; set; }

        public MealPlanType MealPlan { get; set; }

        public UserBootCamp BootCamp { get; set; }

        public WorkoutLevel WorkoutLevel { get; set; }
    }
}
