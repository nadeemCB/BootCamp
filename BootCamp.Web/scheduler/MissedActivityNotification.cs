using BootCamp.Core;
using BootCamp.Core.BoundedContext;
using NLog;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BootCamp.Web.scheduler
{
    public class MissedActivityNotification : IJob
    {
        private readonly IUnitOfWork uow;
        private readonly ILogger logger;
        private readonly IUnitOfWork notificationUow;
        private readonly IUnitOfWork uowUser;
        public MissedActivityNotification()
        {
            uow = new UnitOfWork<BootcampContext>();
            notificationUow = new UnitOfWork<NotificationContext>();
            uowUser = new UnitOfWork<UserContext>();
            logger = LogManager.GetCurrentClassLogger();
        }
        public void Execute(IJobExecutionContext context)
        {
            List< BootCamp.DomainObjects.BootCamp.ActivityLog> activitylogs = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Include(o=>o.BootCampUserDetail.BootCampUser)
                    .Include(o => o.BootCampUserDetail.BootCamp)
                    .Filter(o => o.ActivityStatus == Utils.ActivityStatus.Undefined)
                    .Get().ToList();

            int count = activitylogs.Count();
            int loopCount = 0;
            activitylogs.ForEach(activityLog =>
            {
                loopCount++;
                DateTime bootCampStartDate = activityLog.BootCampUserDetail.BootCamp.StartDate;
                DateTime userDateTime = DateTime.UtcNow.AddMinutes(activityLog.BootCampUserDetail.BootCampUser.TimeZoneOffset);
                DateTime activityLogDate = bootCampStartDate.AddDays(activityLog.DayNo - 1);

                Utils.NotificationType notifyType;
                if(activityLog.LogType == Utils.LogType.Meal)
                {
                    notifyType = Utils.NotificationType.MissedMeal;
                }
                else
                {
                    notifyType = Utils.NotificationType.MissedExercise;
                }

                DomainObjects.UserNotification notification = notificationUow.Repository<DomainObjects.UserNotification>().Query()
                    .Filter(o => o.ForUserId == activityLog.BootCampUserDetail.UserId && o.LogId == activityLog.Id && o.NotificationType == notifyType).Get().FirstOrDefault();

                DomainObjects.UserNotificationSetting settings = uowUser.Repository<DomainObjects.UserNotificationSetting>().FindById(activityLog.BootCampUserDetail.UserId);

                if (notification == null && settings!=null)
                {
                    
                    DateTime activityime = activityLogDate; 
                    if (activityLog.LogType == Utils.LogType.Meal && settings.MealReminder == true)
                    {
                        Utils.MealType mealType = activityLog.MealType.Value;
                        string messageText = string.Empty;
                        if (mealType == Utils.MealType.Breakfast)
                        {
                            activityime = new DateTime(activityLogDate.Year, activityLogDate.Month, activityLogDate.Day, settings.BreakfastTime.Hours, settings.BreakfastTime.Minutes, settings.BreakfastTime.Seconds);
                            messageText = " breakfast?";
                        }
                        else if (mealType == Utils.MealType.Snack1)
                        {
                            activityime = new DateTime(activityLogDate.Year, activityLogDate.Month, activityLogDate.Day, settings.FirstSnackTime.Hours, settings.FirstSnackTime.Minutes, settings.FirstSnackTime.Seconds);
                            messageText = " morning snack?";
                        }
                        else if (mealType == Utils.MealType.Lunch)
                        {
                            activityime = new DateTime(activityLogDate.Year, activityLogDate.Month, activityLogDate.Day, settings.LunchTime.Hours, settings.LunchTime.Minutes, settings.LunchTime.Seconds);
                            messageText = " lunch?";
                        }
                        else if (mealType == Utils.MealType.Snack2)
                        {
                            activityime = new DateTime(activityLogDate.Year, activityLogDate.Month, activityLogDate.Day, settings.SecondSnackTime.Hours, settings.SecondSnackTime.Minutes, settings.SecondSnackTime.Seconds);
                            messageText = " afternoon snack?";
                        }
                        else if (mealType == Utils.MealType.Dinner)
                        {
                            activityime = new DateTime(activityLogDate.Year, activityLogDate.Month, activityLogDate.Day, settings.DinnerTime.Hours, settings.DinnerTime.Minutes, settings.DinnerTime.Seconds);
                            messageText = " dinner?";
                        }
                        if (activityime < userDateTime)
                        {
                            notification = new DomainObjects.UserNotification
                            {
                                ForUserId = activityLog.BootCampUserDetail.BootCampUser.Id,
                                CreatedOn = DateTime.UtcNow,
                                IsDeleted = false,
                                LogId = activityLog.Id,
                                NotificationForId = activityLog.BootCampUserDetail.BootCamp.Id,
                                NotificationType = Utils.NotificationType.MissedMeal,
                                State = ObjectInterfaces.ObjectState.Added,
                                Title = activityLog.BootCampUserDetail.BootCampUser.FullName,
                                UpdatedOn = DateTime.UtcNow,
                                TextLine1 = "Did you miss your" + messageText,
                            };
                            uow.Repository<DomainObjects.UserNotification>().InsertGraph(notification);
                        }
                    }
                    else if(activityLog.LogType == Utils.LogType.Workout && settings.WorkoutReminder == true)
                    {
                        activityime = new DateTime(activityLogDate.Year, activityLogDate.Month, activityLogDate.Day, settings.WorkoutTime.Hours, settings.WorkoutTime.Minutes, settings.WorkoutTime.Seconds);
                        if (activityime < userDateTime)
                        {
                            notification = new DomainObjects.UserNotification
                            {
                                ForUserId = activityLog.BootCampUserDetail.BootCampUser.Id,
                                CreatedOn = DateTime.UtcNow,
                                IsDeleted = false,
                                LogId = activityLog.Id,
                                NotificationForId = activityLog.BootCampUserDetail.BootCamp.Id,
                                NotificationType = Utils.NotificationType.MissedExercise,
                                State = ObjectInterfaces.ObjectState.Added,
                                Title = activityLog.BootCampUserDetail.BootCampUser.FullName,
                                UpdatedOn = DateTime.UtcNow,
                                TextLine1 = "Did you miss your excercise?",
                            };
                            uow.Repository<DomainObjects.UserNotification>().InsertGraph(notification);
                        }
                    }
                }
            });
            uow.Save();
        }
    }
}