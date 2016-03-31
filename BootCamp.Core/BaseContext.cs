using MySql.Data.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Core
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class BaseContext<TContext> : DbContext where TContext : DbContext
    {
        static BaseContext()
        {
            Database.SetInitializer<TContext>(null);
        }
        protected BaseContext() : base("name=" + ConfigurationManager.AppSettings["Environment"] + "") { }
    }
}
