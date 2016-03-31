using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.ObjectInterfaces
{
    public interface IObjectState
    {
        ObjectState State { get; set; }
    }
    public enum ObjectState
    {
        Unchanged,
        Added,
        Modified,
        Deleted
    }
}
