using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using BootCamp.ObjectInterfaces;
using BootCamp.Utils;

namespace BootCamp.DomainObjects
{
    [Table("bc_user_account")]
    public class User:IObjectState
    {
        public User()
        {
            WorkoutLevel = WorkoutLevel.None;
        }
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicture { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool Verified { get; set; }
        public int TimeZoneOffset { get; set; }
        public MealPlanType MealPlan { get; set; }
        public WorkoutLevel WorkoutLevel { get; set; }
        public decimal? AmountPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string DeviceToken { get; set; }
        public virtual IList<Measurement> Measurements { get; set; }
        public virtual IList<UserContacts> UserContacts { get; set; }
        public virtual IList<UserBootCamp> UserBootCamps { get; set; }
        public virtual IList<BootCampSummary> OwnedBootCamps { get; set; }
        public virtual UserNotificationSetting UserNotificationSetting { get; set; }
        [NotMapped]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        [NotMapped]
        public ObjectState State { get; set; }
    }

}
