using BootCamp.DomainObjects;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Core.BoundedContext
{
    public interface IUserContext : IContext
    {
        IDbSet<User> Users { get; set; }
    }
}
