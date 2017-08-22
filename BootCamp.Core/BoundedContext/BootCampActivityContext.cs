using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BootCamp.DomainObjects.BootCampActivity;

namespace BootCamp.Core.BoundedContext
{
    public class BootCampActivityContext : BaseContext<BootCampActivityContext>,IBootCampActivityContext
    {
        public BootCampActivityContext()
        {

        }
        public IDbSet<DomainObjects.BootCampActivity.BootCamp> BootCamps
        {
            get;set;
        }

        public IDbSet<BootCampUserActivity> BootCampUserActivities
        {
            get; set;
        }

        public IDbSet<BootCampUserActivityDetail> BootCampUserActivityDetails
        {
            get; set;
        }

        public IDbSet<BootCampUserDetail> BootCampUserDetails
        {
            get; set;
        }

        public IDbSet<BootCampUser> BootCampUsers
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
