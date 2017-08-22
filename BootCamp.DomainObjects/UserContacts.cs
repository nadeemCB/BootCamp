using BootCamp.ObjectInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects
{
    [Table("bc_user_usercontacts")]
    public class UserContacts
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactNo { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [NotMapped]
        public ObjectState State { get; set; }
    }
}
