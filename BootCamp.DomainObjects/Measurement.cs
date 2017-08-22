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
    [Table("bc_user_measurement")]
    public class Measurement:IObjectState
    {
        [Key]
        public int Id { get; set; }
        
        public int Height { get; set; }
        
        public int Weight { get; set; }
        
        public int GoalWeight { get; set; }
        
        public int Biceps { get; set; }
        
        public int Waist { get; set; }
        
        public int Hips { get; set; }
        
        public int Thighs { get; set; }
        
        public int Chest { get; set; }
        
        public int UpperArm { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [NotMapped]
        public ObjectState State { get; set; }
    }
}
