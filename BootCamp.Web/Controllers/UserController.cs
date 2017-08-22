using BootCamp.Core;
using BootCamp.Core.BoundedContext;
using BootCamp.DomainObjects;
using BootCamp.Helper;
using BootCamp.Models.DTO;
using BootCamp.Utils;
using BootCamp.Web.Helpers;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using NLog;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BootCamp.Web.Infrastructure;
using System.Collections.Generic;
using System.Web.Http.Description;
using BootCamp.Web.Filters;

namespace BootCamp.Web.Controllers
{
    public class UserController : ApiController
    {

        private readonly IUnitOfWork uow;
        private readonly IUnitOfWork BootCampUow;
        private readonly ILogger logger;
        public UserController()
        {
            uow = new UnitOfWork<UserContext>();
            BootCampUow = new UnitOfWork<BootcampContext>();
            logger = LogManager.GetCurrentClassLogger();

        }
        [HttpPost]
        [ActionName("ChangeMealPlan")]
        [ResponseType(typeof(SuccessResponse))]
        [Authorize]
        public HttpResponseMessage ChangeMealPlan(ChangeMealPlanRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;
            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);
            User user = uow.Repository<User>().FindById(userId);
            user.MealPlan = request.MealPlan;
            user.UpdatedOn = DateTime.UtcNow;
            user.State = ObjectInterfaces.ObjectState.Modified;
            uow.Repository<User>().Update(user);
            uow.Save();
            HttpResponseMessage response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("Meal Plan changed"));
            return response;
        }
        [HttpGet]
        [ActionName("GetMealPlan")]
        [ResponseType(typeof(GetMealPlanRequest))]
        [Authorize]
        public HttpResponseMessage GetMealPlan()
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;
            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);
            User user = uow.Repository<User>().FindById(userId);

            HttpResponseMessage response = Request.CreateResponse<GetMealPlanRequest>(HttpStatusCode.OK, new GetMealPlanRequest { MealPlan = user.MealPlan });
            return response;
        }
        [HttpGet]
        [ActionName("GetUserContacts")]
        [ResponseType(typeof(UserContactsResponse))]
        [Authorize]
        public HttpResponseMessage GetUserContacts()
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;
            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);
            User user = uow.Repository<User>().FindById(userId);

            UserContactsResponse userContactsResponse = new UserContactsResponse();

            HttpResponseMessage response = Request.CreateResponse<UserContactsResponse>(HttpStatusCode.OK, userContactsResponse);
            return response;
        }
        [HttpGet]
        [ActionName("GetUserById")]
        [ResponseType(typeof(UserResponse))]
        [Authorize]
        public HttpResponseMessage GetUserById(int id)
        {
            HttpResponseMessage response = null;

            User user = uow.Repository<User>().Query()
                .Include(p => p.Measurements)
                .Include(p => p.UserNotificationSetting)
                .Include(p => p.UserBootCamps.Select(u => u.BootCamp))
                .Filter(o => o.Id == id)
                .Get()
                .FirstOrDefault();
            if (user == null)
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse("User not found."));
            }
            else if (string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName))
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse("User profile not created."));
            }
            else
            {
                List<BootCamp.DomainObjects.BootCamp.ActivityLog> logImages = BootCampUow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserDetail.BootCampUser.Id == id && o.LogType == LogType.Workout && !string.IsNullOrEmpty(o.Image))
                    .Get().OrderBy(o => o.Id).ToList();

                user.Measurements = user.Measurements.OrderBy(o => o.CreatedOn).ToList();
                UserResponse userResponse = new UserResponse
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNo = user.PhoneNumber,
                    Public = user.UserNotificationSetting.IsPublicProfile,
                    ProfilePicture = user.ProfilePicture,
                    Statistics = new UserVitalStatistics
                    {
                        GoalWeight = user.Measurements[0].GoalWeight,
                        Height = user.Measurements[0].Height,
                        Weight = user.Measurements[0].Weight,
                    },
                };

                userResponse.BodyPictures = new List<BodyPicture>();
                logImages.ToList().ForEach(images =>
                {
                    userResponse.BodyPictures.Add(new BodyPicture { ImageName = images.Image, PictureDate = images.LogTime });
                });

                userResponse.Bicep = new List<MeasurementDto>();
                userResponse.Weight = new List<MeasurementDto>();
                userResponse.Chest = new List<MeasurementDto>();
                userResponse.Waist = new List<MeasurementDto>();
                user.Measurements.ToList().ForEach(measurement =>
                {
                    userResponse.Bicep.Add(new MeasurementDto { Date = measurement.CreatedOn, Value = measurement.Biceps });
                    userResponse.Weight.Add(new MeasurementDto { Date = measurement.CreatedOn, Value = measurement.Weight });
                    userResponse.Chest.Add(new MeasurementDto { Date = measurement.CreatedOn, Value = measurement.Chest });
                    userResponse.Waist.Add(new MeasurementDto { Date = measurement.CreatedOn, Value = measurement.Waist });
                });

                userResponse.BootCamps = new List<BootCampHistoryDto>();
                DomainObjects.UserBootCamp currentBootcamp = user.UserBootCamps
                    .Where(o => (o.BootCamp.BootCampStatus != BootCampStatus.Deleted && o.BootCamp.BootCampStatus != BootCampStatus.Banned) 
                    && o.DroppedOut == false 
                    && (o.BootCamp.StartDate <= DateTime.UtcNow && o.BootCamp.EndDate >= DateTime.UtcNow))
                    .OrderByDescending(o => o.BootCamp.StartDate)
                    .FirstOrDefault();
                if (currentBootcamp != null)
                {
                    userResponse.CurrentBootCamp = new BootCampHistoryDto();
                    userResponse.CurrentBootCamp.BootCampId = currentBootcamp.BootCampId;
                    userResponse.CurrentBootCamp.BootCampImage = currentBootcamp.BootCamp.BootcampImage;
                    userResponse.CurrentBootCamp.BootCampName = currentBootcamp.BootCamp.Name;
                    userResponse.CurrentBootCamp.EndDate = currentBootcamp.BootCamp.EndDate;
                    userResponse.CurrentBootCamp.StartDate = currentBootcamp.BootCamp.StartDate;
                    userResponse.CurrentBootCamp.MaxMemebers = currentBootcamp.BootCamp.MaxMembers;
                    userResponse.CurrentBootCamp.TotalMembers = currentBootcamp.BootCamp.SignedUp;
                }

                user.UserBootCamps = user.UserBootCamps
                    .Where(o => (o.BootCamp.BootCampStatus != BootCampStatus.Deleted && o.BootCamp.BootCampStatus != BootCampStatus.Banned)
                    && o.DroppedOut == false && o.BootCamp.EndDate < DateTime.UtcNow)
                    .OrderByDescending(o => o.BootCamp.StartDate)
                    .ToList();
                user.UserBootCamps.ToList().ForEach(bootcamp =>
                {
                    BootCampHistoryDto bootCampHistoryDto = new BootCampHistoryDto();
                    bootCampHistoryDto.BootCampId = bootcamp.BootCampId;
                    bootCampHistoryDto.BootCampImage = bootcamp.BootCamp.BootcampImage;
                    bootCampHistoryDto.BootCampName = bootcamp.BootCamp.Name;
                    bootCampHistoryDto.EndDate = bootcamp.BootCamp.EndDate;
                    bootCampHistoryDto.StartDate = bootcamp.BootCamp.StartDate;
                    bootCampHistoryDto.MaxMemebers = bootcamp.BootCamp.MaxMembers;
                    bootCampHistoryDto.TotalMembers = bootcamp.BootCamp.SignedUp;
                    userResponse.BootCamps.Add(bootCampHistoryDto);
                });
                response = Request.CreateResponse<UserResponse>(HttpStatusCode.OK, userResponse);
            }

            return response;
        }

        [HttpGet]
        [ActionName("GetUser")]
        [ResponseType(typeof(UserResponse))]
        [Authorize]
        public HttpResponseMessage GetUser()
        {
            HttpResponseMessage response = null;
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>().Query()
                .Include(p => p.Measurements)
                .Include(p => p.UserBootCamps.Select(u => u.BootCamp))
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            if (string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName))
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse("User profile not created."));
            }
            else
            {
                List<BootCamp.DomainObjects.BootCamp.ActivityLog> logImages = BootCampUow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserDetail.BootCampUser.Id == userId && o.LogType == LogType.Workout && !string.IsNullOrEmpty(o.Image))
                    .Get().OrderBy(o => o.Id).ToList();

                user.Measurements = user.Measurements.OrderBy(o => o.CreatedOn).ToList();
                UserResponse userResponse = new UserResponse
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNo = user.PhoneNumber,
                    Public = user.UserNotificationSetting.IsPublicProfile,
                    ProfilePicture = user.ProfilePicture,

                    Statistics = new UserVitalStatistics
                    {
                        GoalWeight = user.Measurements[0].GoalWeight,
                        Height = user.Measurements[0].Height,
                        Weight = user.Measurements[0].Weight,
                    },
                };
                userResponse.BootCamps = new List<BootCampHistoryDto>();

                DomainObjects.UserBootCamp currentBootcamp = user.UserBootCamps
                    .Where(o => (o.BootCamp.BootCampStatus != BootCampStatus.Deleted && o.BootCamp.BootCampStatus != BootCampStatus.Banned) 
                    && o.DroppedOut == false 
                    && (o.BootCamp.StartDate <= DateTime.UtcNow && o.BootCamp.EndDate >= DateTime.UtcNow))
                    .OrderByDescending(o => o.BootCamp.StartDate)
                    .FirstOrDefault();
                if (currentBootcamp != null)
                {
                    userResponse.CurrentBootCamp = new BootCampHistoryDto();
                    userResponse.CurrentBootCamp.BootCampId = currentBootcamp.BootCampId;
                    userResponse.CurrentBootCamp.BootCampImage = currentBootcamp.BootCamp.BootcampImage;
                    userResponse.CurrentBootCamp.BootCampName = currentBootcamp.BootCamp.Name;
                    userResponse.CurrentBootCamp.EndDate = currentBootcamp.BootCamp.EndDate;
                    userResponse.CurrentBootCamp.StartDate = currentBootcamp.BootCamp.StartDate;
                    userResponse.CurrentBootCamp.MaxMemebers = currentBootcamp.BootCamp.MaxMembers;
                    userResponse.CurrentBootCamp.TotalMembers = currentBootcamp.BootCamp.SignedUp;
                }
                userResponse.BodyPictures = new List<BodyPicture>();
                logImages.ToList().ForEach(images =>
                {
                    userResponse.BodyPictures.Add(new BodyPicture { ImageName = images.Image, PictureDate = images.LogTime });
                });

                userResponse.Bicep = new List<MeasurementDto>();
                userResponse.Weight = new List<MeasurementDto>();
                userResponse.Chest = new List<MeasurementDto>();
                userResponse.Waist = new List<MeasurementDto>();
                user.Measurements.ToList().ForEach(measurement =>
                {
                    userResponse.Bicep.Add(new MeasurementDto { Date = measurement.CreatedOn, Value = measurement.Biceps });
                    userResponse.Weight.Add(new MeasurementDto { Date = measurement.CreatedOn, Value = measurement.Weight });
                    userResponse.Chest.Add(new MeasurementDto { Date = measurement.CreatedOn, Value = measurement.Chest });
                    userResponse.Waist.Add(new MeasurementDto { Date = measurement.CreatedOn, Value = measurement.Waist });
                });
                user.UserBootCamps = user.UserBootCamps
                    .Where(o => o.BootCamp.BootCampStatus != BootCampStatus.Deleted && o.DroppedOut == false && o.BootCamp.EndDate < DateTime.UtcNow)
                    .OrderByDescending(o => o.BootCamp.StartDate)
                    .ToList();
                user.UserBootCamps.ToList().ForEach(bootcamp =>
                {
                    BootCampHistoryDto bootCampHistoryDto = new BootCampHistoryDto();
                    bootCampHistoryDto.BootCampId = bootcamp.BootCampId;
                    bootCampHistoryDto.BootCampImage = bootcamp.BootCamp.BootcampImage;
                    bootCampHistoryDto.BootCampName = bootcamp.BootCamp.Name;
                    bootCampHistoryDto.EndDate = bootcamp.BootCamp.EndDate;
                    bootCampHistoryDto.StartDate = bootcamp.BootCamp.StartDate;
                    bootCampHistoryDto.MaxMemebers = bootcamp.BootCamp.MaxMembers;
                    bootCampHistoryDto.TotalMembers = bootcamp.BootCamp.SignedUp;
                    userResponse.BootCamps.Add(bootCampHistoryDto);
                });
                response = Request.CreateResponse<UserResponse>(HttpStatusCode.OK, userResponse);
            }

            return response;
        }

        [HttpPost]
        [ActionName("UpdateUser")]
        [ResponseType(typeof(SuccessResponse))]
        [Authorize]
        public HttpResponseMessage UpdateUser(UserUpdateRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>().Query()
                .Include(p => p.Measurements)
                .Include(p => p.UserNotificationSetting)
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            if (user != null)
            {
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.ProfilePicture = request.ProfilePicture;

                user.State = ObjectInterfaces.ObjectState.Modified;
                if (user.Measurements != null)
                {
                    user.Measurements[0].Height = request.Statistics.Height;
                    user.Measurements[0].GoalWeight = request.Statistics.GoalWeight;
                    user.Measurements[0].Weight = request.Statistics.Weight;
                    user.Measurements[0].State = ObjectInterfaces.ObjectState.Modified;
                }
                if (user.UserNotificationSetting != null)
                {
                    user.UserNotificationSetting.IsPublicProfile = request.IsPublicProfile;
                    user.UserNotificationSetting.State = ObjectInterfaces.ObjectState.Modified;
                }
                uow.Repository<User>().Update(user);
                uow.Save();
            }

            HttpResponseMessage response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("Updated"));

            return response;
        }

        [HttpGet]
        [ActionName("GetUserNotificationSetting")]
        [ResponseType(typeof(UserNotificationSettingResponse))]
        [Authorize]
        public HttpResponseMessage GetUserNotificationSetting()
        {
            HttpResponseMessage response = null;
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            UserNotificationSetting userNotification = uow.Repository<UserNotificationSetting>()
                .Query()
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            if (userNotification != null)
            {
                UserNotificationSettingResponse notificationResponse = new UserNotificationSettingResponse
                {
                    IsPublicProfile = userNotification.IsPublicProfile,
                    BootCampInvitations = userNotification.BootCampInvitations,
                    MealReminder = userNotification.MealReminder,
                    WorkoutReminder = userNotification.WorkoutReminder,
                    BreakfastTime = userNotification.BreakfastTime,
                    DinnerTime = userNotification.DinnerTime,
                    FirstSnackTime = userNotification.FirstSnackTime,
                    LunchTime = userNotification.LunchTime,
                    SecondSnackTime = userNotification.SecondSnackTime,
                    WorkoutTime = userNotification.WorkoutTime
                };
                response = Request.CreateResponse<UserNotificationSettingResponse>(HttpStatusCode.OK, notificationResponse);
            }
            return response;
        }

        [HttpPost]
        [ActionName("UpdateNotificationSetting")]
        [ResponseType(typeof(SuccessResponse))]
        [Authorize]
        public HttpResponseMessage UpdateNotificationSetting(UserNotificationSettingRequest request)
        {
            HttpResponseMessage response = null;
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            UserNotificationSetting userNotification = uow.Repository<UserNotificationSetting>()
                .Query()
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            if (userNotification != null)
            {
                userNotification.MealReminder = request.MealReminder;
                userNotification.WorkoutReminder = request.WorkoutReminder;
                userNotification.BootCampInvitations = request.BootCampInvitations;
                userNotification.IsPublicProfile = request.IsPublicProfile;
                userNotification.BreakfastTime = request.BreakfastTime;
                userNotification.DinnerTime = request.DinnerTime;
                userNotification.FirstSnackTime = request.FirstSnackTime;
                userNotification.LunchTime = request.LunchTime;
                userNotification.SecondSnackTime = request.SecondSnackTime;
                userNotification.WorkoutTime = request.WorkoutTime;
                userNotification.UpdatedOn = DateTime.UtcNow;
                userNotification.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<UserNotificationSetting>().Update(userNotification);
                uow.Save();
                response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("Settings updated successfully."));
            }
            else
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.OK, new ErrorResponse("Unable to update settings."));
            }
            return response;
        }

        [HttpGet]
        [ActionName("GetUserInfo")]
        [ResponseType(typeof(UserProfileResponse))]
        [Authorize]
        public HttpResponseMessage GetUserInfo()
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;
            HttpResponseMessage response = null;
            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);
            try
            {
                User user = uow.Repository<User>()
                    .Query()
                    .Include(p => p.UserNotificationSetting)
                    .Include(p => p.UserBootCamps.Select(s => s.BootCamp))
                    .Filter(o => o.Id == userId)
                    .Get()
                    .FirstOrDefault();

                UserProfileResponse userProfile = new UserProfileResponse();
                userProfile.FirstName = user.FirstName;
                userProfile.LastName = user.LastName;
                userProfile.Id = user.Id;
                userProfile.MealPlan = user.MealPlan;
                userProfile.WorkoutLevel = user.WorkoutLevel;
                
                //user.UserBootCamps = user.UserBootCamps.Where(o => o.DroppedOut == false).ToList();

                userProfile.PaidAccount = (user.AmountPaid.HasValue == true) ? true : false;

                List<DomainObjects.UserBootCamp> camps = user.
                    UserBootCamps.
                    Where(o => o.DroppedOut == false &&
                        (o.BootCamp.BootCampStatus != BootCampStatus.Deleted && o.BootCamp.BootCampStatus != BootCampStatus.Banned)).
                    OrderByDescending(p => p.JoinDate).
                    ToList();

                if (user.UserNotificationSetting != null)
                {

                    userProfile.IsPublicProfile = user.UserNotificationSetting.IsPublicProfile;
                    userProfile.MealReminder = user.UserNotificationSetting.MealReminder;
                    userProfile.BootCampInvitations = user.UserNotificationSetting.BootCampInvitations;
                    userProfile.WorkoutReminder = user.UserNotificationSetting.WorkoutReminder;

                }
                if (camps != null && camps.Count() > 0 && 
                    (camps.First().DroppedOut == false && 
                    (camps.First().BootCamp.BootCampStatus != BootCampStatus.Deleted && camps.First().BootCamp.BootCampStatus != BootCampStatus.Banned)))
                {
                    DateTime startDate = camps.First().BootCamp.StartDate;

                    DateTime userDateTime = DateTime.UtcNow.AddMinutes(user.TimeZoneOffset);

                    int DayNo = (userDateTime - startDate).Days + 1;

                    userProfile.BootCamp = new Models.DTO.UserBootCamp
                    {
                        Id = camps.First().BootCampId,
                        IsOwner = camps.First().BootCamp.CreatorId == userId ? true : false,
                        CampImage = camps.First().BootCamp.BootcampImage,
                        CampName = camps.First().BootCamp.Name,
                        StartDate = camps.First().BootCamp.StartDate,
                        EndDate = camps.First().BootCamp.EndDate,
                        IsCheckInDate = DayNo % 7 == 0 ? true : false,
                        Started = camps.First().BootCamp.StartDate > DateTime.Now ? false : true
                    };
                }
                
                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    userProfile.ProfileImage = user.ProfilePicture;
                }
                response = Request.CreateResponse<UserProfileResponse>(HttpStatusCode.OK, userProfile);
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(ex.Message));
            }

            return response;
        }
        [HttpPost]
        [ActionName("ChangePassword")]
        [Authorize]
        [ResponseType(typeof(SuccessResponse))]
        public HttpResponseMessage ChangePassword(ChangePasswordRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>().FindById(userId);

            string oldPassword = EncryptionHelper.DecryptString(user.Password);

            if (oldPassword.Equals(request.OldPassword))
            {
                if (request.NewPassword.Equals(request.ConfirmNewPassword))
                {
                    user.Password = EncryptionHelper.EncryptString(request.NewPassword);
                    user.UpdatedOn = DateTime.UtcNow;
                    user.State = ObjectInterfaces.ObjectState.Modified;
                    uow.Repository<User>().Update(user);
                    uow.Save();
                    return Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("Password Updated successfully."));
                }
                else
                {
                    return Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse("Password and confirm password do not match."));
                }
            }
            else
            {
                return Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse("Old password does not match."));
            }
        }
        [HttpPost]
        [ActionName("NewPassword")]
        [Authorize]
        [ResponseType(typeof(NewPasswordResponse))]
        public HttpResponseMessage NewPassword(UpdatePasswordRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>().FindById(userId);

            if (request.NewPassword.Equals(request.ConfirmNewPassword))
            {
                user.Password = EncryptionHelper.EncryptString(request.NewPassword);
                user.UpdatedOn = DateTime.UtcNow;
                user.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<User>().Update(user);
                uow.Save();
                NewPasswordResponse response = new NewPasswordResponse();
                response.EmailAddress = user.Email;
                return Request.CreateResponse<NewPasswordResponse>(HttpStatusCode.OK, response);
            }
            else
            {
                return Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse("Password and confirm password do not match."));
            }
        }


        [HttpPost]
        [ActionName("ChangeEmail")]
        [Authorize]
        [ResponseType(typeof(SuccessResponse))]
        public HttpResponseMessage ChangeEmail(ChangeEmailRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>().FindById(userId);

            int count = uow.Repository<User>()
                .Query()
                .Filter(o => o.Email.Trim().ToLower() == request.NewEmail.Trim().ToLower() && o.Id != user.Id)
                .Get()
                .Count();

            if (count > 0)
            {
                return Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse("Email address already in use."));
            }
            if (request.OldEmail.Trim().ToLower().Equals(user.Email.Trim().ToLower()))
            {
                user.Email = request.NewEmail;
                user.UpdatedOn = DateTime.UtcNow;
                user.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<User>().Update(user);
                uow.Save();
                return Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("Email Updated successfully."));
            }
            else
            {
                return Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse("Old email does not match."));
            }
        }
        [HttpPost]
        [ActionName("Payment")]
        [ResponseType(typeof(SuccessResponse))]
        [ValidateModel]
        public HttpResponseMessage Payment(PaymentRequest request)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>()
                    .FindById(userId);

            if (user != null)
            {
                user.AmountPaid = request.PaymentAmount;
                user.UpdatedOn = DateTime.UtcNow;
                user.PaymentDate = DateTime.UtcNow;
                user.State = ObjectInterfaces.ObjectState.Modified;

                uow.Repository<User>().Update(user);
                uow.Save();

                response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("User payment updated."));
            }
            else
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.NotFound, new ErrorResponse("User not found."));
            }

            return response;
        }
        /// <summary>
        /// Registers user as unverified. Email address and Phone combination has to be unique.
        /// </summary>
        /// <param name="userRegisteration"></param>
        /// <returns></returns>
        /// 
        [HttpPost]
        [ActionName("RegisterUser")]
        [ResponseType(typeof(RegisterUserResponse))]
        public HttpResponseMessage RegisterUser(UserRegisteration userRegisteration)
        {
            HttpResponseMessage response = null;
            if (ModelState.IsValid)
            {
                List<User> users = uow.Repository<User>()
                    .Query()
                    .Filter(o => o.Email.ToLower().Trim().Equals(userRegisteration.Email.ToLower().Trim()) || o.PhoneNumber.Trim().Equals(userRegisteration.PhoneNumber.Trim()))
                    .Get()
                    .ToList();
                User user = null;
                if (users == null || users.Count == 0)
                {
                    user = new DomainObjects.User
                    {
                        Email = userRegisteration.Email.Trim(),
                        Password = EncryptionHelper.EncryptString(userRegisteration.Password.Trim()),
                        PhoneNumber = userRegisteration.PhoneNumber.Trim(),
                        TimeZoneOffset = userRegisteration.TimeZoneOffSet,
                        CreatedOn = System.DateTime.UtcNow,
                        UpdatedOn = System.DateTime.UtcNow,
                        Verified = false,
                        DeviceToken = userRegisteration.DeviceToken,
                        State = ObjectInterfaces.ObjectState.Added
                    };
                    uow.Repository<User>().Insert(user);
                    uow.Save();
                    response = Request.CreateResponse<RegisterUserResponse>(HttpStatusCode.OK, new RegisterUserResponse { Id = user.Id, Message = Resources.StringResources.AccountCreated });
                }
                else if (users != null && users.Count > 0)
                {
                    user = users.Where(o => o.Email.ToLower().Trim().Equals(userRegisteration.Email.ToLower().Trim()) || o.PhoneNumber.Trim().Equals(userRegisteration.PhoneNumber.Trim())).FirstOrDefault();
                    if (user != null && user.Verified == false)
                    {
                        response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(Resources.StringResources.AccountVerifyError));
                    }
                    else if (user != null && user.Verified == true)
                    {
                        response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(Resources.StringResources.EmailExistsError));
                    }
                }
            }
            else
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(ErrorHelpers.ModelStateErrors(ModelState)));
            }
            return response;
        }
        [HttpPost]
        [Authorize]
        [ActionName("CreateUserProfile")]
        public HttpResponseMessage CreateUserProfile(UserProfile userProfile)
        {
            HttpResponseMessage response = null;
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            claim = currentClaimsPrincipal.FindFirst(ClaimTypes.Email);
            string email = claim.Value;

            User user = uow.Repository<User>().Query().Filter(o => o.Email.ToLower().Trim().Equals(email.ToLower().Trim())).Get().FirstOrDefault();
            DateTime opTime = DateTime.UtcNow;
            if (user != null && user.Verified == true)
            {
                user.FirstName = userProfile.FirstName;
                user.LastName = userProfile.LastName;
                user.ProfilePicture = string.IsNullOrEmpty(userProfile.ImageName) ? null : userProfile.ImageName;
                user.UpdatedOn = opTime;
                user.MealPlan = userProfile.MealPlan;
                user.WorkoutLevel = userProfile.WorkoutLevel;
                user.State = ObjectInterfaces.ObjectState.Modified;

                user.Measurements = new List<BootCamp.DomainObjects.Measurement>();
                user.Measurements.Add(new DomainObjects.Measurement
                {
                    Biceps = userProfile.Measurment.Biceps,
                    Chest = userProfile.Measurment.Chest,
                    GoalWeight = userProfile.Measurment.GoalWeight,
                    Height = userProfile.Measurment.Height,
                    Hips = userProfile.Measurment.Hips,
                    Thighs = userProfile.Measurment.Thighs,
                    UpperArm = userProfile.Measurment.UpperArm,
                    Waist = userProfile.Measurment.Waist,
                    Weight = userProfile.Measurment.Weight,
                    CreatedOn = opTime,
                    UpdatedOn = opTime,
                    User = user,

                    UserId = user.Id,
                    State = ObjectInterfaces.ObjectState.Added
                });

                user.UserNotificationSetting = new UserNotificationSetting
                {
                    User = user,
                    Id = user.Id,
                    BootCampInvitations = true,
                    MealReminder = true,

                    WorkoutReminder = true,
                    IsPublicProfile = true,
                    BreakfastTime = new TimeSpan(8, 0, 0),
                    FirstSnackTime = new TimeSpan(10, 0, 0),
                    LunchTime = new TimeSpan(12, 0, 0),
                    SecondSnackTime = new TimeSpan(15, 0, 0),
                    DinnerTime = new TimeSpan(19, 0, 0),
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    State = ObjectInterfaces.ObjectState.Added
                };

                uow.Repository<User>().Update(user);
                uow.Save();
            }
            else
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.NotFound, new ErrorResponse("User not found."));
            }

            response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse("User profile created."));
            return response;
        }
        [HttpPost]
        [ActionName("VerifyUser")]
        [ResponseType(typeof(TokenResponse))]
        public HttpResponseMessage VerifyUser(VerifyUser verifyUser)
        {
            HttpResponseMessage response = null;
            if (ModelState.IsValid)
            {
                User user = uow.Repository<User>().Query().Filter(o => o.Email.ToLower().Trim().Equals(verifyUser.Email.ToLower().Trim())).Get().FirstOrDefault();
                if (user != null && user.Verified == false)
                {
                    user.Verified = true;
                    user.UpdatedOn = System.DateTime.UtcNow;
                    user.State = ObjectInterfaces.ObjectState.Modified;
                    uow.Repository<User>().Update(user);
                    uow.Save();

                    string token = TokenGenerator.ProcessRequest(user);

                    response = Request.CreateResponse<TokenResponse>(HttpStatusCode.OK, new TokenResponse(Resources.StringResources.AccountVerified, token));
                }
                else if (user == null)
                {
                    response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(Resources.StringResources.EmailNotExistError));
                }
                else if (user.Verified == true)
                {
                    response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(Resources.StringResources.AccountAlreadyVerifiedError));
                }
            }
            else
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(ErrorHelpers.ModelStateErrors(ModelState)));
            }
            return response;
        }

        [HttpPost]
        [Authorize]
        [ActionName("SyncContacts")]
        [ResponseType(typeof(UserContactsResponse))]
        public HttpResponseMessage SyncContacts(SyncUserRequest userContactsRequest)
        {
            HttpResponseMessage response = null;
            try
            {
                ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

                Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
                int userId = int.Parse(claim.Value);

                claim = currentClaimsPrincipal.FindFirst(ClaimTypes.MobilePhone);
                string phoneNo = claim.Value;

                UserContactsResponse userContactsResponse = new UserContactsResponse();
                if (userContactsRequest.UserContacts != null)
                {
                    userContactsRequest.UserContacts.ForEach((userContact) =>
                    {
                        UserContactsResponseDto userContactResponse = new UserContactsResponseDto();
                        userContactResponse.Name = userContact.Name;
                        userContactResponse.PhoneNumber = userContact.PhoneNumber;
                        if (!phoneNo.Trim().Equals(userContactResponse.PhoneNumber.Trim()))
                        {
                            User user = uow.Repository<User>()
                                .Query()
                                .Include(p => p.UserNotificationSetting)
                                .Include(p => p.UserBootCamps.Select(t=>t.BootCamp))
                                .Filter(o => o.PhoneNumber.Trim().Equals(userContact.PhoneNumber.Trim()) 
                                        && (o.UserNotificationSetting.BootCampInvitations == true))
                                .Get()
                                .FirstOrDefault();
                            if (user != null && user.UserNotificationSetting != null && user.UserNotificationSetting.BootCampInvitations == true)
                            {
                                if(user.UserBootCamps != null)
                                {
                                    user.UserBootCamps = user.UserBootCamps.Where(o => o.DroppedOut == false && (o.BootCamp.BootCampStatus != BootCampStatus.Banned && o.BootCamp.BootCampStatus != BootCampStatus.Deleted)).ToList();

                                    int count = user.UserBootCamps.Where(o => System.DateTime.UtcNow.Date < o.BootCamp.EndDate.Date).Count();

                                    if (count == 0)
                                    {
                                        userContactResponse.Id = user.Id;
                                        userContactsResponse.AppUsers.Add(userContactResponse);
                                    }
                                }
                            }
                            else
                            {
                                userContactsResponse.OtherUsers.Add(userContactResponse);
                            }
                        }
                    });
                }
                response = Request.CreateResponse<UserContactsResponse>(HttpStatusCode.OK, userContactsResponse);
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(ex.Message));
            }

            return response;
        }
        [HttpPost]
        [Authorize]
        [ActionName("GetUsersForInvite")]
        [ResponseType(typeof(UserContactsResponse))]
        public HttpResponseMessage GetUsersForInvite(SyncUserRequest userContactsRequest)
        {
            HttpResponseMessage response = null;
            try
            {
                ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

                Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
                int userId = int.Parse(claim.Value);

                claim = currentClaimsPrincipal.FindFirst(ClaimTypes.MobilePhone);
                string phoneNo = claim.Value;

                UserContactsResponse userContactsResponse = new UserContactsResponse();
                if (userContactsRequest.UserContacts != null)
                {
                    userContactsRequest.UserContacts.ForEach((userContact) =>
                    {
                        if (!(string.IsNullOrEmpty(userContact.PhoneNumber) || string.IsNullOrWhiteSpace(userContact.PhoneNumber)))
                        {
                            UserContactsResponseDto userContactResponse = new UserContactsResponseDto();
                            userContactResponse.Name = userContact.Name;
                            userContactResponse.PhoneNumber = userContact.PhoneNumber;
                            if (!phoneNo.Trim().Equals(userContactResponse.PhoneNumber.Trim()))
                            {
                                User user = uow.Repository<User>()
                                    .Query()
                                    .Include(p => p.UserNotificationSetting)
                                    .Include(p => p.UserBootCamps.Select(t => t.BootCamp))
                                    .Filter(o => o.PhoneNumber.Trim().Equals(userContact.PhoneNumber.Trim()) 
                                        && (o.UserNotificationSetting.BootCampInvitations == true))
                                        .Get()
                                        .FirstOrDefault();

                                if (user != null && user.UserBootCamps != null)
                                {
                                    DomainObjects.UserBootCamp bootCamp = user.UserBootCamps.Where(p => p.DroppedOut == false && p.BootCamp.EndDate > DateTime.Now).FirstOrDefault();
                                    if (bootCamp == null)
                                    {
                                        userContactResponse.Id = user.Id;
                                        userContactsResponse.AppUsers.Add(userContactResponse);
                                    }
                                }
                                else
                                {
                                    userContactsResponse.OtherUsers.Add(userContactResponse);
                                }
                            }
                        }
                    });
                }
                response = Request.CreateResponse<UserContactsResponse>(HttpStatusCode.OK, userContactsResponse);
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(ex.Message));
            }

            return response;
        }

        [HttpPost]
        [ActionName("LoginUser")]
        [ResponseType(typeof(LoginResponse))]
        public HttpResponseMessage LoginUser(UserLogin userLogin)
        {
            HttpResponseMessage response = null;
            if (ModelState.IsValid)
            {
                User user = uow.Repository<User>().Query().Filter(o => o.Email.Trim().ToLower().Equals(userLogin.Email.ToLower().Trim())).Get().FirstOrDefault();
                if (user != null && EncryptionHelper.DecryptString(user.Password).ToLower().Trim().Equals(userLogin.Password.Trim().ToLower())
                    && user.Verified == true)
                {
                    string token = TokenGenerator.ProcessRequest(user);
                    string profilePic = string.Empty;
                    bool profileCreated = true;
                    if (user.ProfilePicture != null && !user.ProfilePicture.Equals(""))
                    {
                        profilePic = user.ProfilePicture;
                    }
                    if (user.FirstName == null)
                    {
                        profileCreated = false;
                    }
                    user.DeviceToken = userLogin.DeviceToken;
                    user.State = ObjectInterfaces.ObjectState.Modified;
                    user.UpdatedOn = DateTime.UtcNow;
                    uow.Repository<User>().Update(user);
                    uow.Save();
                    response = Request.CreateResponse<LoginResponse>(HttpStatusCode.OK, new LoginResponse(Resources.StringResources.LoginSuccess, token, profilePic, profileCreated, user.Id));
                }
                else if (user != null && EncryptionHelper.DecryptString(user.Password).ToLower().Trim().Equals(userLogin.Password.Trim().ToLower())
                    && user.Verified == false)
                {
                    response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(Resources.StringResources.AccountVerifyError));
                }
                else
                {
                    response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(Resources.StringResources.LoginCredentialsError));
                }
            }
            else
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(ErrorHelpers.ModelStateErrors(ModelState)));
            }
            return response;

        }
        [HttpPost]
        [Authorize]
        [ActionName("UploadImage")]
        public async Task<FileUploadResponse> UploadImage()
        {
            var uploadPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["UserImages"]);

            var multipartFormDataStreamProvider = new UploadMultipartFormProvider(uploadPath);

            // Read the MIME multipart asynchronously 
            await Request.Content.ReadAsMultipartAsync(multipartFormDataStreamProvider);

            string _localFileName = multipartFormDataStreamProvider
                .FileData.Select(multiPartData => multiPartData.LocalFileName).FirstOrDefault();

            // Create response
            return new FileUploadResponse
            {
                Url = (ConfigurationManager.AppSettings["UserImages"] + "//" + Path.GetFileName(_localFileName)).ToAbsoluteUrl(),
                FileName = Path.GetFileName(_localFileName),
                FileLength = new FileInfo(_localFileName).Length
            };
        }
        [HttpPost]
        [Authorize]
        [ActionName("ChangeDifficultyLevel")]
        public HttpResponseMessage ChangeDifficultyLevel(ChangeDifficultyLevelRequest request)
        {
            HttpResponseMessage response = null;
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>().FindById(userId);
            if (user != null)
            {
                user.WorkoutLevel = request.WorkoutLevel;

                user.UpdatedOn = DateTime.UtcNow;
                user.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<User>().Update(user);
                uow.Save();
            }
            response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse());
            return response;
        }
        [HttpPost]
        [Authorize]
        [ActionName("UpdateDeviceToken")]
        public HttpResponseMessage UpdateDeviceToken(string token)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>().FindById(userId);

            if (user != null)
            {
                user.DeviceToken = token;
                user.UpdatedOn = DateTime.UtcNow;
                user.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<User>().Update(user);
                uow.Save();
            }

            HttpResponseMessage response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse());
            return response;
        }

        [HttpPost]
        [Authorize]
        [ActionName("Logout")]
        public HttpResponseMessage Logout()
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = uow.Repository<User>().FindById(userId);

            if (user != null)
            {
                user.DeviceToken = string.Empty;
                user.UpdatedOn = DateTime.UtcNow;
                user.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<User>().Update(user);
                uow.Save();
            }

            HttpResponseMessage response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse());
            return response;
        }

        //[HttpPost]
        //[ActionName("BlockUser")]
        //public HttpResponseMessage BlockUser(BlockUserRequest request)
        //{

        //}

        [HttpPost]
        [ActionName("ForgotPassword")]
        public HttpResponseMessage ForgotPassword(ForgotPassword forgotPassword)
        {
            HttpResponseMessage response = null;
            if (ModelState.IsValid)
            {
                User user = uow.Repository<User>().Query().Filter(o => o.Email.ToLower().Trim().Equals(forgotPassword.Email.ToLower().Trim())).Get().FirstOrDefault();
                if (user != null && user.Verified == true)
                {
                    string body = string.Empty;
                    using (StreamReader sr = new StreamReader(HostingEnvironment.MapPath("\\App_Data\\Templates\\" + ConfigurationManager.AppSettings["ForgotPasswordTemplate"] + "")))
                    {
                        body = sr.ReadToEnd();
                    }
                    string img = ConfigurationManager.AppSettings["EmailImagePath"];
                    string webLink = ConfigurationManager.AppSettings["WebLink"];
                    string name = user.FullName;
                    string email = user.Email;
                    string password = EncryptionHelper.DecryptString(user.Password);
                    string token = TokenGenerator.ProcessRequest(user);
                    string messageBody = string.Format(body, img, name, "bootcamp://code/" + token, webLink + token);

                    MailHelper mailHelper = new MailHelper
                    {
                        Body = messageBody,
                        Recipient = user.Email,
                        RecipientCC = null,
                        Subject = @"Forgot Password"
                    };
                    try
                    {
                        mailHelper.Send();
                        response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, new SuccessResponse());
                    }
                    catch (Exception ex)
                    {
                        logger.Log<Exception>(LogLevel.Error, ex);
                        response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.InternalServerError, new ErrorResponse(Resources.StringResources.EmailSendingError));
                    }
                }
                else
                {
                    response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(Resources.StringResources.EmailNotExistError));
                }
            }
            else
            {
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, new ErrorResponse(ErrorHelpers.ModelStateErrors(ModelState)));
            }
            return response;
        }
   }
}
