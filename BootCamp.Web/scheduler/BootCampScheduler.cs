using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BootCamp.Web.scheduler
{
    public class BootCampScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail activityjob = JobBuilder.Create<BootCampJob>().Build();
            IJobDetail missedActivityjob = JobBuilder.Create<MissedActivityNotification>().Build();
            IJobDetail pushNotificationjob = JobBuilder.Create<PushNotificationjob>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("activityjob", "group1")
                .StartNow()
                .WithSimpleSchedule(x=>x.WithIntervalInMinutes(120).RepeatForever())
                //.WithDailyTimeIntervalSchedule
                //  (s =>
                //     s.WithIntervalInHours(1)
                //    .OnEveryDay()
                //    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(utcNow.Hour, utcNow.Minute))
                //  )
                .Build();

            ITrigger missedActivityTrigger = TriggerBuilder.Create()
                .WithIdentity("missedActivityjob", "group2")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever())
                //.WithDailyTimeIntervalSchedule
                //  (s =>
                //     s.WithIntervalInHours(1)
                //    .OnEveryDay()
                //    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(utcNow.Hour, utcNow.Minute))
                //  )
                .Build();

            ITrigger pushNotificationTrigger = TriggerBuilder.Create()
                .WithIdentity("pushNotificationjob", "group3")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever())
                //.WithDailyTimeIntervalSchedule
                //  (s =>
                //     s.WithIntervalInHours(1)
                //    .OnEveryDay()
                //    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(utcNow.Hour, utcNow.Minute))
                //  )
                .Build();

            scheduler.ScheduleJob(activityjob, trigger);
            scheduler.ScheduleJob(missedActivityjob, missedActivityTrigger);
            scheduler.ScheduleJob(pushNotificationjob, pushNotificationTrigger);
            //scheduler.TriggerJob(job.Key);
        }
    }
}