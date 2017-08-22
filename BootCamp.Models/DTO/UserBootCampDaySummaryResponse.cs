using BootCamp.Models.Meal;
using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class UserBootCampDaySummaryResponse
    {
        public DateTime SummaryDay { get; set; }
        public WorkoutSummary WorkoutSummary { get; set; }
        public List<MealSummary> MealSummary { get; set; }
    }

    public class MealSummary
    {
        public int Id { get; set; }
        public MealType MealType { get; set; }
        public DateTime MealTime { get; set; }
        public ActivityStatus MealTaken { get; set; }
        public string Image { get; set; }
        public List<weeksWeekDayMealRecipe> Recipies { get; set; }
    }

    public class RecipieDetailDto
    {
        public string Name { get; set; }
    }

    public class WorkoutSummary
    {
        public int Id { get; set; }
        public DateTime ExerciseTime { get; set; }
        public string Image { get; set; }
        public string WorkoutName { get; set; }
        public List<ExcerciseDetailDto> Excercises { get; set; }
        public int CompletionTime { get; set; }
    }

    public class ExcerciseDetailDto
    {
        public string Name { get; set; }
        public string SetsReps { get; set; }
        public int? Time { get; set; }
    }
}
