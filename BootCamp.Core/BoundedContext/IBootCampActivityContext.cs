using BootCamp.DomainObjects.BootCampActivity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Core.BoundedContext
{
    public interface IBootCampActivityContext:IContext
    {
        IDbSet<BootCamp.DomainObjects.BootCampActivity.BootCamp> BootCamps { get; set; }
        IDbSet<BootCampUser> BootCampUsers { get; set; }
        IDbSet<BootCampUserDetail> BootCampUserDetails { get; set; }
        IDbSet<BootCampUserActivity> BootCampUserActivities { get; set; }
        IDbSet<BootCampUserActivityDetail> BootCampUserActivityDetails { get; set; }
    }
}
