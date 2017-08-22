using BootCamp.ObjectInterfaces;
using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.BootCamp
{
    [Table("bc_meal")]
    public class Meal: IObjectState
    {
        [Key]
        public int Id { get; set; }
        public MealPlanType DietTypeId { get; set; }
        public int WeekNumber { get; set; }
        public int DayOfWeek { get; set; }
        public int MealSequence { get; set; }
        public int RecipeId { get; set; }
        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }
        public MealType MealType { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;set;
        }
    }
}
