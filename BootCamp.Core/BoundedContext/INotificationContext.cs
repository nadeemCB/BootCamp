using BootCamp.DomainObjects;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Core.BoundedContext
{
    public interface INotificationContext: IContext
    {
        IDbSet<UserNotification> UserNotifications { get; set; }
    }
}
