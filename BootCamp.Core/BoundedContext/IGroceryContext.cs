using BootCamp.DomainObjects.Grocery;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Core.BoundedContext
{
    public interface IGroceryContext:IContext
    {
        IDbSet<GroceryCategory> GroceryCategories { get; set; }
        IDbSet<GroceryItem> GroceryItems { get; set; }
    }
}
