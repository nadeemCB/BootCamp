using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class Measurement
    {
        [Required(ErrorMessageResourceName = "HeightError", ErrorMessageResourceType =typeof(Resources.StringResources))]
        public int Height { get; set; }
        [Required(ErrorMessageResourceName = "WeightError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public int Weight { get; set; }
        [Required(ErrorMessageResourceName = "GoalWeightError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public int GoalWeight { get; set; }
        [Required(ErrorMessageResourceName = "BicepsError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public int Biceps { get; set; }
        [Required(ErrorMessageResourceName = "WaistError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public int Waist { get; set; }
        [Required(ErrorMessageResourceName = "HipsError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public int Hips { get; set; }
        [Required(ErrorMessageResourceName = "ThighsError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public int Thighs { get; set; }
        [Required(ErrorMessageResourceName = "ChestError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public int Chest { get; set; }
        [Required(ErrorMessageResourceName = "UpperArmError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public int UpperArm { get; set; }
    }
}
