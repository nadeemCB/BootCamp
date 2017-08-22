using BootCamp.Core;
using BootCamp.Core.BoundedContext;
using BootCamp.Models.Meal;
using BootCamp.Utils;
using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace BootCamp.Web.scheduler
{
    public class BootCampJob : IJob
    {
        private readonly IUnitOfWork uow;
        private readonly ILogger logger;

        public BootCampJob()
        {
            uow = new UnitOfWork<BootcampContext>();
            logger = LogManager.GetCurrentClassLogger();
        }

        public void Execute(IJobExecutionContext context)
        {
            DateTimeOffset nextOffset;
            if (context.NextFireTimeUtc.HasValue)
            {
                nextOffset = context.NextFireTimeUtc.Value;
            }

            DateTime currentUtcDate = DateTime.UtcNow.Date;

            List<BootCamp.DomainObjects.BootCamp.BootCamp> bootCamps = uow.Repository<BootCamp.DomainObjects.BootCamp.BootCamp>().Query()
                .Include(t=>t.BootCampUserDetails)
                .Filter(o => o.StartDate <= currentUtcDate && o.EndDate >= currentUtcDate && o.BootCampStatus == BootCampStatus.Active)
                .Get()
                .ToList();
            int bootCampCount = bootCamps.Count();


            bootCamps.ForEach((bootCamp) =>
            {
                int DayNo = ((DateTime.UtcNow.Date - bootCamp.StartDate).Days) + 1;
                int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
                int weekDayNo = DayNo - (7 * (weekNo - 1));

                List<BootCamp.DomainObjects.BootCamp.BootCampUserDetail> bootCampUsersDetails = bootCamp.BootCampUserDetails.Where(o => o.DroppedOut == false).ToList();
                int userCount = bootCampUsersDetails.Count();
                var DayMeals = uow.Repository<BootCamp.DomainObjects.BootCamp.Meal>().Query()
                    .Filter(o => o.DayOfWeek == weekDayNo && o.WeekNumber == weekNo)
                    .Get()
                    .Select(o => new { o.MealSequence, o.MealType })
                    .Distinct().ToList();


                bootCampUsersDetails.ToList().ForEach(bootCampUser =>
                {
                    int count = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserId == bootCampUser.Id && o.DayNo == DayNo && o.WeekDayNo == weekDayNo && o.WeekNo == weekNo)
                    .Get().Count();
                    if (count == 0)
                    {
                        List<int> mealsPerDay = DayMeals.Select(o => o.MealSequence).Distinct().ToList();
                        mealsPerDay.ForEach(mealPerDay =>
                        {
                            MealType mealType = DayMeals.Where(o => o.MealSequence == mealPerDay).Select(t => t.MealType).FirstOrDefault();
                            BootCamp.DomainObjects.BootCamp.ActivityLog activityLog = new DomainObjects.BootCamp.ActivityLog();
                            activityLog.DayNo = DayNo;
                            activityLog.MealType = mealType;
                            if (mealType == MealType.Breakfast)
                            {
                                activityLog.ActivityStartTime = new TimeSpan(6, 0, 0);
                                activityLog.ActivityEndTime = new TimeSpan(9, 0, 0);
                            }
                            else if (mealType == MealType.Snack1)
                            {
                                activityLog.ActivityStartTime = new TimeSpan(10, 0, 0);
                                activityLog.ActivityEndTime = new TimeSpan(12, 0, 0);
                            }
                            else if (mealType == MealType.Lunch)
                            {
                                activityLog.ActivityStartTime = new TimeSpan(13, 0, 0);
                                activityLog.ActivityEndTime = new TimeSpan(15, 0, 0);
                            }
                            else if (mealType == MealType.Snack2)
                            {
                                activityLog.ActivityStartTime = new TimeSpan(16, 0, 0);
                                activityLog.ActivityEndTime = new TimeSpan(17, 0, 0);
                            }
                            else if (mealType == MealType.Dinner)
                            {
                                activityLog.ActivityStartTime = new TimeSpan(18, 0, 0);
                                activityLog.ActivityEndTime = new TimeSpan(20, 0, 0);
                            }
                            activityLog.ActivityStatus = ActivityStatus.Undefined;

                            activityLog.LogPrivacyStatus = LogPrivacyStatus.Undefined;
                            activityLog.LogType = LogType.Meal;
                            activityLog.WeekDayNo = weekDayNo;
                            activityLog.WeekNo = weekNo;
                            activityLog.BootCampUserId = bootCampUser.Id;
                            activityLog.State = ObjectInterfaces.ObjectState.Added;
                            uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().InsertGraph(activityLog);
                        });
                        BootCamp.DomainObjects.BootCamp.ActivityLog workoutActivityLog = new DomainObjects.BootCamp.ActivityLog();
                        workoutActivityLog.DayNo = DayNo;
                        workoutActivityLog.ActivityStartTime = new TimeSpan(6, 0, 0);
                        workoutActivityLog.ActivityEndTime = new TimeSpan(23, 0, 0);
                        workoutActivityLog.ActivityStatus = ActivityStatus.Undefined;
                        workoutActivityLog.LogPrivacyStatus = LogPrivacyStatus.Undefined;
                        workoutActivityLog.LogType = LogType.Workout;
                        workoutActivityLog.WeekDayNo = weekDayNo;
                        workoutActivityLog.WeekNo = weekNo;
                        workoutActivityLog.BootCampUserId = bootCampUser.Id;
                        workoutActivityLog.State = ObjectInterfaces.ObjectState.Added;
                        uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().InsertGraph(workoutActivityLog);
                    }
                });
            });
            uow.Save();
            logger.Log(LogLevel.Info, "Service ran at :" + DateTime.UtcNow);
        }
    }
    public class DailyMealType
    {
        public int MealSequence { get; set; }
        public MealType MealType { get; set; }
    }
}