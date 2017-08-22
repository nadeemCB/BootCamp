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
    [Table("bc_exercise")]
    public class Exercise: IObjectState
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string SetsRep { get; set; }
        public Nullable<int> Time { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public int WorkoutId { get; set; }
        [ForeignKey("WorkoutId")]
        public Workout Workout { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;set;
        }
    }
}
