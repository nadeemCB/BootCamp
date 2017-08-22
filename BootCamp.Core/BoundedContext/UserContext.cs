using BootCamp.DomainObjects;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Core.BoundedContext
{
    public class UserContext : BaseContext<UserContext>, IUserContext
    {
        public IDbSet<User> Users
        {
            get;
            set;
        }
        public IDbSet<Measurement> Measurements
        {
            get;
            set;
        }

        public IDbSet<BootCampSummary> BootCampSummarys
        {
            get;
            set;
        }

        public IDbSet<UserBootCamp> UserBootCamps
        {
            get;
            set;
        }

        public IDbSet<UserNotificationSetting> UserNotificationSettings
        {
            get;
            set;
        }

        public override int SaveChanges()
        {
            this.ApplyStateChanges();
            return base.SaveChanges();
        }
        public void SetAdd(object entity)
        {
            Entry(entity).State = EntityState.Added;
        }

        public void SetModified(object entity)
        {
            Entry(entity).State = EntityState.Modified;
        }
        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }
    }
}
