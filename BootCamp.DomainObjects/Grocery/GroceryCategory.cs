using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.Grocery
{
    [Table("bc_grocery_category")]
    public class GroceryCategory
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<GroceryItem> GroceryItems { get; set; }
    }
}
