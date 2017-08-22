using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class MissedMealResponse
    {
        public MissedMealSummary MealSummary { get; set; }
    }
    public class MissedMealSummary
    {
        public int Id { get; set; }
        public MealType MealType { get; set; }
        public DateTime MealTime { get; set; }
        public ActivityStatus MealTaken { get; set; }
        public string Image { get; set; }
        public List<MissedRecipieDetailDto> Recipies { get; set; }
    }

    public class MissedRecipieDetailDto
    {
        public string Name { get; set; }
    }
}
