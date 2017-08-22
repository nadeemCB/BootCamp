using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class UserProfile
    {
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,45}$")]
        [Required]
        public string FirstName { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z''-'\s]{1,45}$")]
        public string LastName { get; set; }
        public MealPlanType MealPlan { get; set; }
        public string ImageName { get; set; }
        public WorkoutLevel WorkoutLevel { get; set; }
        [Required(ErrorMessageResourceName = "MeasurementError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public Measurement Measurment { get; set; }
    }
}
