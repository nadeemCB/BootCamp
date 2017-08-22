using BootCamp.ObjectInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.BootCampActivity
{
    [Table("bc_user_account")]
    public class BootCampUser : IObjectState
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePicture { get; set; }
        public decimal? AmountPaid { get; set; }
        public int TimeZoneOffset { get; set; }
        public virtual IList<BootCampUserDetail> BootCampUserDetails { get; set; }
        [NotMapped]
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
        [NotMapped]
        public ObjectState State
        {
            get;
            set;
        }
    }
}
