using BootCamp.Core;
using BootCamp.Core.BoundedContext;
using BootCamp.DomainObjects;
using BootCamp.Models.DTO;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using BootCamp.Web.Helpers;

namespace BootCamp.Web.Controllers
{
    public class NotificationController : ApiController
    {
        private readonly IUnitOfWork uow;
        private readonly ILogger logger;
        private readonly IUnitOfWork userUow;
        private readonly IUnitOfWork bootCampUow;
        public NotificationController()
        {
            uow = new UnitOfWork<NotificationContext>();
            userUow = new UnitOfWork<UserContext>();
            bootCampUow = new UnitOfWork<BootcampContext>();
            logger = LogManager.GetCurrentClassLogger();
        }
        [HttpGet]
        [ActionName("GetMyNotifications")]
        [ResponseType(typeof(NotificationResponse))]
        [Authorize]
        public HttpResponseMessage GetMyNotifications()
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);
            User user = userUow.Repository<User>().FindById(userId);
            List<UserNotification> notificationsList = uow.Repository<UserNotification>()
                .Query()
                .Include(p=>p.BootCamp)
                .Filter(o => o.ForUserId == userId && o.IsDeleted == false)
                .Get()
                .OrderByDescending(o=>o.CreatedOn)
                .ToList();
            NotificationResponse notificationResponse = new NotificationResponse();
            notificationResponse.Notifications = new List<Notification>();
            notificationsList.ForEach((userNotification) =>
            {
                Notification notification = new Notification();
                notification.Id = userNotification.Id;
                if (userNotification.NotificationForId.HasValue)
                {
                    notification.BootCampId = userNotification.BootCamp.Id;
                    if (userNotification.BootCamp.StartDate.Date < DateTime.Now.Date)
                    {
                        notification.IsStarted = true;
                    }
                    else
                    {
                        notification.IsStarted = false;
                    }
                }
                if(userNotification.LogId.HasValue)
                {
                    notification.ActivityLogId = userNotification.LogId.Value;
                }
                
                notification.Title = user.FullName;               
                notification.TextLine1 = userNotification.TextLine1;
                notification.TextLine2 = userNotification.TextLine2;
                notification.NotificationType = userNotification.NotificationType;
                notification.NotificationDateTime = userNotification.CreatedOn;
                double minuteDiff = DateTime.Now.Subtract(userNotification.CreatedOn).TotalMinutes;
                notification.NotificationTime = minuteDiff.ToString() + " mins";
                if (userNotification.SentByUserId > 0)
                {
                    User sentBy = userUow.Repository<User>().Query()
                        .Filter(o => o.Id == userNotification.SentByUserId)
                        .Get()
                        .FirstOrDefault();
                    notification.Image = string.IsNullOrEmpty(sentBy.ProfilePicture) == true ? "" : sentBy.ProfilePicture;
                }
                notificationResponse.Notifications.Add(notification);
            });

            response = Request.CreateResponse<NotificationResponse>(HttpStatusCode.OK, notificationResponse);

            return response;
        }
        [HttpPost]
        [ActionName("DeleteNotification")]
        [ResponseType(typeof(SuccessResponse))]
        [Authorize]
        public HttpResponseMessage DeleteNotification(DeleteNotificationRequest request)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            UserNotification notification = uow.Repository<UserNotification>().FindById(request.NotificationId);

            notification.IsDeleted = true;
            notification.State = ObjectInterfaces.ObjectState.Modified;

            uow.Repository<UserNotification>().Update(notification);

            uow.Save();

            BootCamp.DomainObjects.BootCamp.BootCampInvitedUsers inivte = bootCampUow.Repository<BootCamp.DomainObjects.BootCamp.BootCampInvitedUsers>()
                .Query()
                .Filter(o=>o.UserId == userId && o.BootCampId == notification.NotificationForId)
                .Get()
                .FirstOrDefault();
            if(inivte!= null)
            {
                inivte.State = ObjectInterfaces.ObjectState.Deleted;
                bootCampUow.Repository<BootCamp.DomainObjects.BootCamp.BootCampInvitedUsers>().Delete(inivte);
                bootCampUow.Save();
            }

            response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("Notification Deleted."));

            return response;
        }
        [HttpPost]
        [ActionName("DeleteAllNotifications")]
        [ResponseType(typeof(SuccessResponse))]
        [Authorize]
        public HttpResponseMessage DeleteAllNotifications()
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            List<UserNotification> notifications = uow.Repository<UserNotification>().Query()
                .Filter(o => o.ForUserId == userId && o.IsDeleted == false)
                .Get()
                .ToList();
            notifications.ForEach((notification) => 
            {
                notification.IsDeleted = true;
                notification.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<UserNotification>().Update(notification);

                BootCamp.DomainObjects.BootCamp.BootCampInvitedUsers inivte = bootCampUow.Repository<BootCamp.DomainObjects.BootCamp.BootCampInvitedUsers>()
                .Query()
                .Filter(o => o.UserId == userId && o.BootCampId == notification.NotificationForId)
                .Get()
                .FirstOrDefault();
                if (inivte != null)
                {
                    inivte.State = ObjectInterfaces.ObjectState.Deleted;
                    bootCampUow.Repository<BootCamp.DomainObjects.BootCamp.BootCampInvitedUsers>().Delete(inivte);

                }

            });
            
            uow.Save();
            bootCampUow.Save();
            response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("Notifications Deleted."));

            return response;
        }
    }
}
