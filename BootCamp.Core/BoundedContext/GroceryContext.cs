using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BootCamp.DomainObjects.Grocery;

namespace BootCamp.Core.BoundedContext
{
    public class GroceryContext : BaseContext<GroceryContext>, IGroceryContext
    {
        public IDbSet<GroceryCategory> GroceryCategories
        {
            get; set;
        }

        public IDbSet<GroceryItem> GroceryItems
        {
            get; set;
        }



        public override int SaveChanges()
        {
            this.ApplyStateChanges();
            return base.SaveChanges();
        }

        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }
    }
}
