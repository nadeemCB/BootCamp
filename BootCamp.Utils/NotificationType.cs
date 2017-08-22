using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Utils
{
    public enum NotificationType
    {
        JoinBootCamp = 0,
        WorkOut = 1,
        Meal = 2,
        WeeklyCheckin = 3,
        MissedMeal = 4,
        MissedExercise = 5,
        BootCampDeleted = 6,
    }
    public enum MealPlanType
    {
        Clean = 0,
        Lean = 1,
    }
    public enum MealType
    {
        Breakfast = 0,
        Snack1 = 1,
        Lunch = 2,
        Dinner = 3,
        Snack2 = 4,
        Undefined = 5
    }
    public enum MarkAs
    {
        Favorite = 0,
        UnFavorite = 1
    }
    public enum LogPrivacyStatus
    {
        Public = 0,
        Private = 1,
        LeaderOnly = 2,
        Undefined = 3
    }
    public enum ActivityStatus
    {
        Taken = 0,
        Skipped = 1,
        Undefined = 2
    }
    public enum LogType
    {
        Meal = 0,
        Workout = 1,
    }
    public enum WorkoutType
    {
        Home = 0,
        Gym = 1,
        NotSet = 2
    }
    public enum WorkoutLevel
    {
        Beginner = 0,
        Intermediate = 1,
        Expert = 2,
        None = 3
    }
    public enum BootCampStatus
    {
        Active = 0,
        Deleted = 1,
        Banned = 2,
        Completed = 3
    }
}
