using BootCamp.DomainObjects;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bootcamp = BootCamp.DomainObjects.BootCamp;
namespace BootCamp.Core.BoundedContext
{
    public interface IBootcampContext : IContext
    {
        IDbSet<Bootcamp.BootCamp> BootCamps { get; set; }
        IDbSet<Bootcamp.BootCampUser> BootCampUsers { get; set; }
        IDbSet<Bootcamp.BootCampInvitedUsers> BootCampInvitations { get; set; }
        IDbSet<Bootcamp.BootCampUserDetail> BootCampUserDetails { get; set; }
        IDbSet<Bootcamp.Meal> Meals { get; set; }
        IDbSet<Bootcamp.Recipe> Recipes { get; set; }
        IDbSet<Bootcamp.RecipeIngredient> RecipeIngredients { get; set; }
        IDbSet<Bootcamp.RecipeInstruction> RecipeInstructions { get; set; }
        IDbSet<UserNotification> UserNotifications { get; set; }
        IDbSet<Bootcamp.FavoriteRecipe> FavoriteRecipes { get; set; }
        IDbSet<Bootcamp.ActivityLog> ActivityLogs { get; set; }
        IDbSet<Bootcamp.BootCampUserNotifications> BootCampUserNotifications { get; set; }
        IDbSet<Bootcamp.Exercise> Exercises { get; set; }
        IDbSet<Bootcamp.Workout> Workouts { get; set; }
        IDbSet<Bootcamp.WeeklyCheckIn> WeeklyCheckIns { get; set; }
        IDbSet<Bootcamp.BootCampReport> BootCampReport { get; set; }
    }
}
