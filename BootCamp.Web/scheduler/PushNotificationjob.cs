using BootCamp.Core;
using BootCamp.Core.BoundedContext;
using BootCamp.DomainObjects;
using BootCamp.Utils;
using Newtonsoft.Json.Linq;
using NLog;
using PushSharp.Apple;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace BootCamp.Web.scheduler
{
    public class PushNotificationjob : IJob
    {
        private readonly IUnitOfWork uow;
        private readonly ILogger logger;
        private readonly IUnitOfWork notificationUow;
        public PushNotificationjob()
        {
            notificationUow = new UnitOfWork<NotificationContext>();
            uow = new UnitOfWork<UserContext>();
            logger = LogManager.GetCurrentClassLogger();
        }
        public void Execute(IJobExecutionContext context)
        {
            List<UserNotification> notificationsList = notificationUow.Repository<UserNotification>()
                .Query()
                .Include(p => p.BootCamp)
                .Filter(o => (o.PushSent.HasValue == false) && o.IsDeleted == false)
                .Get()
                .OrderBy(o => o.ForUserId)
                .ToList();
            int userId = 0;
            User user = null;
            if (notificationsList != null && notificationsList.Count > 0)
            {
                ApnsConfiguration config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                HostingEnvironment.MapPath("\\App_Data\\" + ConfigurationManager.AppSettings["PushCertificate"]), "");
                ApnsServiceBroker apnsBroker = new ApnsServiceBroker(config);

                apnsBroker.OnNotificationFailed += ApnsBroker_OnNotificationFailed;

                apnsBroker.OnNotificationSucceeded += ApnsBroker_OnNotificationSucceeded;

                apnsBroker.Start();

                notificationsList.ForEach(notification =>
                {
                    if (notification.ForUserId != userId)
                    {
                        user = uow.Repository<User>().FindById(notification.ForUserId);
                    }
                    if (user != null && !string.IsNullOrEmpty(user.DeviceToken) && (DateTime.UtcNow.AddMinutes(user.TimeZoneOffset).Date == notification.CreatedOn.Date))
                    {
                        Pushobject obj = new Pushobject();
                        obj.aps = new Aps();
                        obj.aps.badge = 0;
                        obj.aps.alert = new Alert();
                        obj.aps.alert.body = notification.TextLine1 + " "+ notification.TextLine2;
                        obj.aps.alert.title = notification.Title;
                        obj.Data = new NotificationData();
                        obj.Data.Id = notification.Id;

                        if (notification.NotificationForId.HasValue)
                        {
                            obj.Data.BootCampId = notification.BootCamp.Id;
                            if (notification.BootCamp.StartDate.Date < DateTime.Now.Date)
                            {
                                obj.Data.IsStarted = true;
                            }
                            else
                            {
                                obj.Data.IsStarted = false;
                            }
                        }
                        if (notification.LogId.HasValue)
                        {
                            obj.Data.ActivityLogId = notification.LogId.Value;
                        }
                        obj.Data.Title = notification.Title;
                        obj.Data.TextLine1 = notification.TextLine1;
                        obj.Data.TextLine2 = notification.TextLine2;
                        obj.Data.NotificationType = notification.NotificationType;
                        double minuteDiff = DateTime.Now.Subtract(notification.CreatedOn).TotalMinutes;
                        obj.Data.NotificationTime = minuteDiff.ToString() + " mins";

                        apnsBroker.QueueNotification(new ApnsNotification
                        {
                            DeviceToken = user.DeviceToken,
                            Payload = JObject.FromObject(obj)
                        });
                        notification.PushSent = true;
                        notification.UpdatedOn = DateTime.UtcNow;
                        notification.State = ObjectInterfaces.ObjectState.Modified;
                    }
                    else if(user != null && DateTime.UtcNow.AddMinutes(user.TimeZoneOffset).Date > notification.CreatedOn.Date)
                    {
                        notification.PushSent = false;
                        notification.UpdatedOn = DateTime.UtcNow;
                        notification.State = ObjectInterfaces.ObjectState.Modified;
                    }
                });
                apnsBroker.Stop();
            }
            notificationUow.Save();
        }

        private void ApnsBroker_OnNotificationSucceeded(ApnsNotification notification)
        {
            //Pushobject obj = notification.Payload.ToObject<Pushobject>();
            //UserNotification userNotification = notificationUow.Repository<UserNotification>().FindById(obj.Data.Id);
            //userNotification.PushSent = true;
            //userNotification.UpdatedOn = DateTime.UtcNow;
            //userNotification.State = ObjectInterfaces.ObjectState.Modified;
            //notificationUow.Repository<UserNotification>().Update(userNotification);
            //notificationUow.Save();
            logger.Log(LogLevel.Info, "Notification Sent");
        }

        private void ApnsBroker_OnNotificationFailed(ApnsNotification notification, AggregateException exception)
        {
            //Pushobject obj = notification.Payload.ToObject<Pushobject>();
            //UserNotification userNotification = notificationUow.Repository<UserNotification>().FindById(obj.Data.Id);
            //userNotification.PushSent = false;
            //userNotification.UpdatedOn = DateTime.UtcNow;
            //userNotification.State = ObjectInterfaces.ObjectState.Modified;
            //notificationUow.Repository<UserNotification>().Update(userNotification);
            //notificationUow.Save();
            Console.WriteLine("Apple Notification Sent!");
            exception.Handle(ex =>
            {
                logger.Log(LogLevel.Error, ex);
                // See what kind of exception it was to further diagnose
                if (ex is ApnsNotificationException)
                {
                    var notificationException = (ApnsNotificationException)ex;

                    // Deal with the failed notification
                    var apnsNotification = notificationException.Notification;
                    var statusCode = notificationException.ErrorStatusCode;

                    Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                }
                else
                {
                    // Inner exception might hold more useful information like an ApnsConnectionException           
                    Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                }

                // Mark it as handled
                return true;
            });
        }
    }


    public class Pushobject
    {
        public Aps aps { get; set; }
        public NotificationData Data { get; set; }
    }

    public class NotificationData
    {
        public long Id { get; set; }
        public int BootCampId { get; set; }
        public int ActivityLogId { get; set; }
        public string Title { get; set; }
        public string TextLine1 { get; set; }
        public string TextLine2 { get; set; }
        public string NotificationTime { get; set; }
        public NotificationType NotificationType { get; set; }
        public bool IsStarted { get; set; }
    }

    public class Aps
    {
        public Alert alert { get; set; }
        public int badge { get; set; }
    }

    public class Alert
    {
        public string title { get; set; }
        public string body { get; set; }
    }

}