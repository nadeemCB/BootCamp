using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Core
{
    public interface IUnitOfWork
    {
        void Dispose();
        int Save();
        void Dispose(bool disposing);
        IRepository<T> Repository<T>() where T : class;
    }
}
