using BootCamp.ObjectInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.BootCamp
{
    [Table("bc_user_notificationsetting")]
    public class BootCampUserNotifications: IObjectState
    {
        [Key, ForeignKey("User")]
        public int Id { get; set; }
        public bool BootCampInvitations { get; set; }
        public bool MealReminder { get; set; }
        public bool WorkoutReminder { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        [Column("Public")]
        public bool IsPublicProfile { get; set; }
        public virtual BootCampUser User { get; set; }
        public TimeSpan BreakfastTime { get; set; }
        public TimeSpan FirstSnackTime { get; set; }
        public TimeSpan LunchTime { get; set; }
        public TimeSpan SecondSnackTime { get; set; }
        public TimeSpan DinnerTime { get; set; }
        public TimeSpan WorkoutTime { get; set; }
        [NotMapped]
        public ObjectState State { get; set; }
    }
    
    
}
