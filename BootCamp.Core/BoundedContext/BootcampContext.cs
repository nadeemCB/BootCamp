using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BootCamp.DomainObjects;
using Bootcamp = BootCamp.DomainObjects.BootCamp;

namespace BootCamp.Core.BoundedContext
{
    public class BootcampContext : BaseContext<BootcampContext>, IBootcampContext
    {
        public BootcampContext()
        {
            
        }
        public IDbSet<Bootcamp.BootCamp> BootCamps
        {
            get;set;
        }

        public IDbSet<Bootcamp.BootCampUser> BootCampUsers
        {
            get; set;
        }
        public IDbSet<Bootcamp.BootCampInvitedUsers> BootCampInvitations
        {
            get; set;
        }

        public IDbSet<Bootcamp.BootCampUserDetail> BootCampUserDetails
        {
            get; set;
        }

        public IDbSet<UserNotification> UserNotifications
        {
            get; set;
        }

        public IDbSet<Bootcamp.Meal> Meals
        {
            get; set;
        }

        public IDbSet<Bootcamp.Recipe> Recipes
        {
            get; set;
        }

        public IDbSet<Bootcamp.RecipeIngredient> RecipeIngredients
        {
            get; set;
        }

        public IDbSet<Bootcamp.RecipeInstruction> RecipeInstructions
        {
            get; set;
        }

        public IDbSet<Bootcamp.FavoriteRecipe> FavoriteRecipes
        {
            get; set;
        }

        public IDbSet<Bootcamp.ActivityLog> ActivityLogs
        {
            get; set;
        }

        public IDbSet<Bootcamp.BootCampUserNotifications> BootCampUserNotifications
        {
            get; set;
        }

        public IDbSet<Bootcamp.Exercise> Exercises
        {
            get; set;
        }

        public IDbSet<Bootcamp.Workout> Workouts
        {
            get; set;
        }

        public IDbSet<Bootcamp.WeeklyCheckIn> WeeklyCheckIns
        {
            get; set;
        }

        public IDbSet<Bootcamp.BootCampReport> BootCampReport
        {
            get; set;
        }

        public override int SaveChanges()
        {
            this.ApplyStateChanges();
            return base.SaveChanges();
        }
        public void SetAdd(object entity)
        {
            Entry(entity).State = EntityState.Added;
        }

        public void SetModified(object entity)
        {
            Entry(entity).State = EntityState.Modified;
        }

        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }
    }
}
