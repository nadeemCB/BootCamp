using BootCamp.ObjectInterfaces;
using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.Grocery
{
    [Table("bc_grocery_item")]
    public class GroceryItem
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public GroceryCategory GroceryCategory { get; set; }
        public string GroceryItemName { get; set; }
        public int WeekNumber { get; set; }
        public MealPlanType MealPlanType { get; set; }
        [NotMapped]
        public ObjectState State { get; set; }
    }
}
