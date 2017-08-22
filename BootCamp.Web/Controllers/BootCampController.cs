using BootCamp.Core;
using BootCamp.Core.BoundedContext;
using BootcampDto = BootCamp.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Bootcamp = BootCamp.DomainObjects.BootCamp;
using System.Security.Claims;
using System.Web;
using BootCamp.Web.Helpers;
using System.Threading.Tasks;
using System.Configuration;
using BootCamp.Web.Infrastructure;
using System.IO;
using System.Data.Entity;
using System.Web.Http.Description;
using BootCamp.Models.Meal;
using BootCamp.DomainObjects;
using BootCamp.Utils;
using NLog;
using BootCamp.Web.Filters;
using BootCamp.DomainObjects.Grocery;
using System.Linq.Expressions;

namespace BootCamp.Web.Controllers
{
    public class BootCampController : ApiController
    {
        private readonly IUnitOfWork uow;
        private readonly IUnitOfWork groceryUow;
        private readonly IUnitOfWork notificationUow;
        private readonly IUnitOfWork userUow;
        private readonly ILogger logger;
        private const int MAXMEMBERS = 15;
        public BootCampController()
        {
            uow = new UnitOfWork<BootcampContext>();
            groceryUow = new UnitOfWork<GroceryContext>();
            notificationUow = new UnitOfWork<NotificationContext>();
            userUow = new UnitOfWork<UserContext>();
            logger = LogManager.GetCurrentClassLogger();
        }
        [HttpPost]
        [ActionName("LogWeeklyWeight")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        [Authorize]
        public HttpResponseMessage LogWeeklyWeight(BootcampDto.LogWeeklyWeightRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Include(o => o.BootCampUser)
                .Filter(o => o.BootCampId == request.BootCampId && o.UserId == userId)
                .Get()
                .FirstOrDefault();

            Bootcamp.WeeklyCheckIn weeklyCheckin = uow.Repository<Bootcamp.WeeklyCheckIn>().Query()
                .Filter(o => o.UserId == bootCampUserDetail.Id && o.WeekNo == request.WeekNo)
                .Get()
                .FirstOrDefault();

            if(weeklyCheckin != null)
            {
                weeklyCheckin.Weight = true;
                weeklyCheckin.WeekNo = request.WeekNo;
                weeklyCheckin.UpdatedOn = DateTime.UtcNow;
                weeklyCheckin.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<Bootcamp.WeeklyCheckIn>().Update(weeklyCheckin);
                uow.Save();
            }
            else
            {
                weeklyCheckin = new Bootcamp.WeeklyCheckIn();
                weeklyCheckin.Weight = true;
                weeklyCheckin.UserId = bootCampUserDetail.Id;
                weeklyCheckin.WeekNo = request.WeekNo;
                weeklyCheckin.UpdatedOn = DateTime.UtcNow;
                weeklyCheckin.CreatedOn = DateTime.UtcNow;
                weeklyCheckin.State = ObjectInterfaces.ObjectState.Added;
                uow.Repository<Bootcamp.WeeklyCheckIn>().Insert(weeklyCheckin);
                uow.Save();
            }
            Measurement measurement = new Measurement
            {
                UserId = userId,
                Weight = request.Weight,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                State = ObjectInterfaces.ObjectState.Added
            };
            userUow.Repository<Measurement>().Insert(measurement);
            userUow.Save();

            HttpResponseMessage response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse());
            return response;
        }

        [HttpGet]
        [ActionName("GetGroceryList")]
        [ResponseType(typeof(BootcampDto.GroceryListResponse))]
        [Authorize]
        public HttpResponseMessage GetGroceryList(int BootCampId)
        {
            HttpResponseMessage response = Request.CreateResponse<BootcampDto.GroceryListResponse>(HttpStatusCode.OK, new BootcampDto.GroceryListResponse());
            return response;
        }
        [HttpGet]
        [ActionName("GetWeeklyGroceryList")]
        [ResponseType(typeof(BootcampDto.GroceryListResponse))]
        [Authorize]
        public HttpResponseMessage GetWeeklyGroceryList(int BootCampId,int weekNo)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Include(o=>o.BootCampUser)
                .Filter(o => o.BootCampId == BootCampId && o.UserId == userId)
                .Get()
                .FirstOrDefault();

            List<GroceryItem> groceryItems = groceryUow.Repository<GroceryItem>()
                .Query()
                .Include(o => o.GroceryCategory)
                .Filter(o => o.WeekNumber == weekNo && o.MealPlanType == bootCampUserDetail.BootCampUser.MealPlan)
                .Get()
                .ToList();

            BootcampDto.GroceryListResponse response = new BootcampDto.GroceryListResponse();
            response.ItemCategories = new List<BootcampDto.ItemCategoryDto>();

            List<int> categories = groceryItems.Select(t => t.CategoryId).Distinct().ToList();

            categories.ForEach(category =>
            {
                List<BootcampDto.CategoryItemDto> itemDtos = new List<BootcampDto.CategoryItemDto>();
                List<GroceryItem> catGroceryItems = groceryItems.Where(o => o.CategoryId == category).ToList();
                string catName = catGroceryItems.First().GroceryCategory.Name;
                catGroceryItems.ForEach(item =>
                {
                    itemDtos.Add(new BootcampDto.CategoryItemDto { Name = item.GroceryItemName });

                });
                response.ItemCategories.Add(new BootcampDto.ItemCategoryDto { ItemCategory = catName, CategoryItems = itemDtos });
            });

            HttpResponseMessage res = Request.CreateResponse<BootcampDto.GroceryListResponse>(HttpStatusCode.OK, response);
            return res;
        }
        [HttpGet]
        [ActionName("GetActivityLogById")]
        [ResponseType(typeof(BootcampDto.LogActivityResponse))]
        [Authorize]
        public HttpResponseMessage GetActivityLogById(int logId)
        {
            Bootcamp.ActivityLog activitylog = uow.Repository<Bootcamp.ActivityLog>().FindById(logId);
            BootcampDto.LogActivityResponse logResponse = new BootcampDto.LogActivityResponse();
            if (activitylog != null)
            {
                logResponse.ActivityStatus = activitylog.ActivityStatus;
                logResponse.Description = activitylog.Description;
                if (activitylog.Duration.HasValue)
                {
                    logResponse.Duration = activitylog.Duration.Value;
                }
                logResponse.Id = activitylog.Id;
                logResponse.Image = activitylog.Image;
                logResponse.LogStatus = activitylog.LogPrivacyStatus;
                logResponse.LogType = activitylog.LogType;
            }
            HttpResponseMessage response = Request.CreateResponse<BootcampDto.LogActivityResponse>(HttpStatusCode.OK, logResponse);
            return response;
        }
        [HttpPost]
        [ActionName("LogWeeklyMeasurements")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        [Authorize]
        public HttpResponseMessage LogWeeklyMeasurements(BootcampDto.LogWeeklyMeasurementRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Include(o => o.BootCampUser)
                .Filter(o => o.BootCampId == request.BootCampId && o.UserId == userId)
                .Get()
                .FirstOrDefault();

            Bootcamp.WeeklyCheckIn weeklyCheckin = uow.Repository<Bootcamp.WeeklyCheckIn>().Query()
                .Filter(o => o.UserId == bootCampUserDetail.Id && o.WeekNo == request.WeekNo)
                .Get()
                .FirstOrDefault();

            if (weeklyCheckin != null)
            {
                weeklyCheckin.Measurements = true;
                weeklyCheckin.WeekNo = request.WeekNo;
                weeklyCheckin.UpdatedOn = DateTime.UtcNow;
                weeklyCheckin.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<Bootcamp.WeeklyCheckIn>().Update(weeklyCheckin);
                uow.Save();
            }
            else
            {
                weeklyCheckin = new Bootcamp.WeeklyCheckIn();
                weeklyCheckin.Measurements = true;
                weeklyCheckin.UserId = bootCampUserDetail.Id;
                weeklyCheckin.WeekNo = request.WeekNo;
                weeklyCheckin.UpdatedOn = DateTime.UtcNow;
                weeklyCheckin.CreatedOn = DateTime.UtcNow;
                weeklyCheckin.State = ObjectInterfaces.ObjectState.Added;
                uow.Repository<Bootcamp.WeeklyCheckIn>().Insert(weeklyCheckin);
                uow.Save();
            }
            Measurement measurement = new Measurement
            {
                UserId = userId,
                Biceps = request.Biceps,
                Chest = request.Chest,
                Waist = request.Waist,
                Thighs = request.Thigh,
                Hips = request.Hip,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                State = ObjectInterfaces.ObjectState.Added
            };
            userUow.Repository<Measurement>().Insert(measurement);
            userUow.Save();

            HttpResponseMessage response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse());
            return response;
        }

        [HttpPost]
        [ActionName("LogWeeklyImages")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        [Authorize]
        public HttpResponseMessage LogWeeklyImages(BootcampDto.LogWeeklyBodyImagesRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Include(o => o.BootCampUser)
                .Filter(o => o.BootCampId == request.BootCampId && o.UserId == userId)
                .Get()
                .FirstOrDefault();

            Bootcamp.WeeklyCheckIn weeklyCheckin = uow.Repository<Bootcamp.WeeklyCheckIn>().Query()
                .Filter(o => o.UserId == bootCampUserDetail.Id && o.WeekNo == request.WeekNo)
                .Get()
                .FirstOrDefault();

            if (weeklyCheckin != null)
            {
                weeklyCheckin.BodyImages = true;
                weeklyCheckin.WeekNo = request.WeekNo;
                weeklyCheckin.UpdatedOn = DateTime.UtcNow;
                weeklyCheckin.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<Bootcamp.WeeklyCheckIn>().Update(weeklyCheckin);
                uow.Save();
            }
            else
            {
                weeklyCheckin = new Bootcamp.WeeklyCheckIn();
                weeklyCheckin.BodyImages = true;
                weeklyCheckin.UserId = bootCampUserDetail.Id;
                weeklyCheckin.WeekNo = request.WeekNo;
                weeklyCheckin.UpdatedOn = DateTime.UtcNow;
                weeklyCheckin.CreatedOn = DateTime.UtcNow;
                weeklyCheckin.State = ObjectInterfaces.ObjectState.Added;
                uow.Repository<Bootcamp.WeeklyCheckIn>().Insert(weeklyCheckin);
                uow.Save();
            }

            HttpResponseMessage response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse());
            return response;
        }

        [HttpPost]
        [ActionName("LogWeeklyGrocery")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        [Authorize]
        public HttpResponseMessage LogWeeklyGrocery(BootcampDto.LogWeeklyGroceryRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Include(o => o.BootCampUser)
                .Filter(o => o.BootCampId == request.BootCampId && o.UserId == userId)
                .Get()
                .FirstOrDefault();

            Bootcamp.WeeklyCheckIn weeklyCheckin = uow.Repository<Bootcamp.WeeklyCheckIn>().Query()
                .Filter(o => o.UserId == bootCampUserDetail.Id && o.WeekNo == request.WeekNo)
                .Get()
                .FirstOrDefault();

            if (weeklyCheckin != null)
            {
                weeklyCheckin.Grocery = true;
                weeklyCheckin.UpdatedOn = DateTime.UtcNow;
                weeklyCheckin.WeekNo = request.WeekNo;

                weeklyCheckin.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<Bootcamp.WeeklyCheckIn>().Update(weeklyCheckin);
                uow.Save();
            }
            else
            {
                weeklyCheckin = new Bootcamp.WeeklyCheckIn();
                weeklyCheckin.Grocery = true;
                weeklyCheckin.UpdatedOn = DateTime.UtcNow;
                weeklyCheckin.CreatedOn = DateTime.UtcNow;
                weeklyCheckin.UserId = bootCampUserDetail.Id;
                weeklyCheckin.WeekNo = request.WeekNo;
                weeklyCheckin.State = ObjectInterfaces.ObjectState.Added;
                uow.Repository<Bootcamp.WeeklyCheckIn>().Insert(weeklyCheckin);
                uow.Save();
            }
            HttpResponseMessage response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse());
            return response;
        }

        [HttpGet]
        [ActionName("WeeklyCheckinStatus")]
        [ResponseType(typeof(BootcampDto.WeeklyCheckinStatusResponse))]
        [Authorize]
        public HttpResponseMessage WeeklyCheckinStatus(int bootCampId)
        {
            BootcampDto.WeeklyCheckinStatusResponse response = new BootcampDto.WeeklyCheckinStatusResponse();
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Include(o => o.BootCampUser)
                .Filter(o => o.BootCampId == bootCampId && o.UserId == userId)
                .Get()
                .FirstOrDefault();

            List<Bootcamp.WeeklyCheckIn> weeklyCheckins = uow.Repository<Bootcamp.WeeklyCheckIn>().Query()
                .Filter(o => o.UserId == bootCampUserDetail.Id)
                .Get()
                .ToList();
            if(weeklyCheckins != null)
            {
                response.Data = new List<BootcampDto.CheckInLogData>(weeklyCheckins.Count());
                weeklyCheckins.ForEach(weeklyCheckin =>
                {
                    BootcampDto.CheckInLogData data = new BootcampDto.CheckInLogData();
                    data.Week = weeklyCheckin.WeekNo;
                    data.WeightLogged = weeklyCheckin.Weight;
                    data.ShoppingListLogged = weeklyCheckin.Grocery;
                    data.MeasurementsLogged = weeklyCheckin.Measurements;
                    data.BodyPicturesLogged = weeklyCheckin.BodyImages;
                    response.Data.Add(data);
                });
            }
            return Request.CreateResponse<BootcampDto.WeeklyCheckinStatusResponse>(HttpStatusCode.OK, response);
        }
        [HttpPost]
        [ActionName("MarkRecipie")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        [Authorize]
        public HttpResponseMessage MarkRecipie(BootcampDto.MarkRecipieRequest request)
        {
            BootcampDto.SuccessResponse response = new BootcampDto.SuccessResponse();
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.FavoriteRecipe favRecipe = uow.Repository<Bootcamp.FavoriteRecipe>().Query()
                    .Filter(o => o.UserId == userId && o.RecipeId == request.RecipieId).Get().FirstOrDefault();
            if(favRecipe == null)
            {
                favRecipe = new DomainObjects.BootCamp.FavoriteRecipe();
                favRecipe.Favorite = request.MarkAs;
                favRecipe.RecipeId = request.RecipieId;
                favRecipe.UserId = userId;
                favRecipe.State = ObjectInterfaces.ObjectState.Added;
                uow.Repository<Bootcamp.FavoriteRecipe>().Insert(favRecipe);
                uow.Save();
                response.Message = "Favorite updated.";
            }
            else
            {
                favRecipe.Favorite = request.MarkAs;
                favRecipe.RecipeId = request.RecipieId;
                favRecipe.UserId = userId;
                favRecipe.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<Bootcamp.FavoriteRecipe>().Update(favRecipe);
                uow.Save();
                response.Message = "Favorite updated.";
            }
            HttpResponseMessage httpResponse = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse());
            return httpResponse;
        }

        [HttpGet]
        [ActionName("GetFavoriteRecipieList")]
        [ResponseType(typeof(BootcampDto.FavoriteRecipieListResponse))]
        [Authorize]
        public HttpResponseMessage GetFavoriteRecipieList()
        {
            BootcampDto.FavoriteRecipieListResponse recipeList = new BootcampDto.FavoriteRecipieListResponse();
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            List<Bootcamp.FavoriteRecipe> favRecipes = uow.Repository<Bootcamp.FavoriteRecipe>().Query()
                .Include(t=>t.Recipe)
                .Filter(o => o.UserId == userId && o.Favorite == MarkAs.Favorite).Get().ToList();

            if(favRecipes != null && favRecipes.Count > 0)
            {
                recipeList.Data = new BootcampDto.FavoriteRecipieListData();
                recipeList.Data.FavoriteRecipieList = new List<BootcampDto.FavoriteRecipieDto>();
                favRecipes.ForEach(favRecipe =>
                {
                    recipeList.Data.FavoriteRecipieList.Add(new BootcampDto.FavoriteRecipieDto { Id = favRecipe.Recipe.Id,Name = favRecipe.Recipe.Name });
                });
            }
            HttpResponseMessage response = Request.CreateResponse<BootcampDto.FavoriteRecipieListResponse>(HttpStatusCode.OK, recipeList);
            return response;
        }

        [HttpGet]
        [ActionName("GetFavoriteRecipie")]
        [ResponseType(typeof(weeksWeekDayMealRecipe))]
        public HttpResponseMessage GetFavoriteRecipie(int Id)
        {
            HttpResponseMessage response = null;
            weeksWeekDayMealRecipe Recipe = new weeksWeekDayMealRecipe();
            Bootcamp.Recipe favRecipe = uow.Repository<Bootcamp.Recipe>().Query()
                .Include(t => t.RecipeIngredients)
                .Include(t => t.RecipeInstructions)
                .Filter(o => o.Id == Id).Get().FirstOrDefault();

            if (favRecipe != null)
            {
                Recipe.image = favRecipe.Image;
                Recipe.Id = favRecipe.Id;
                if(favRecipe.Cooktime.HasValue)
                Recipe.cooktime = favRecipe.Cooktime.Value;
                Recipe.name = favRecipe.Name;
                Recipe.IsFavorite = true;
                if(favRecipe.Preptime.HasValue)
                Recipe.preptime = favRecipe.Preptime.Value;
                if(favRecipe.Readintime.HasValue)
                Recipe.readintime = favRecipe.Readintime.Value;
                if (favRecipe.RecipeIngredients != null && favRecipe.RecipeIngredients.Count > 0)
                {
                    Recipe.ingredients = new weeksWeekDayMealRecipeIngredient[favRecipe.RecipeIngredients.Count];
                    for (int l = 0; l < Recipe.ingredients.Length; l++)
                    {
                        Recipe.ingredients[l] = new weeksWeekDayMealRecipeIngredient();
                        Recipe.ingredients[l].name = favRecipe.RecipeIngredients[l].Ingredient;
                    }
                }
                if (favRecipe.RecipeInstructions != null && favRecipe.RecipeInstructions.Count > 0)
                {
                    Recipe.directions = new weeksWeekDayMealRecipeDirection[favRecipe.RecipeInstructions.Count];
                    for (int l = 0; l < Recipe.directions.Length; l++)
                    {
                        Recipe.directions[l] = new weeksWeekDayMealRecipeDirection();
                        Recipe.directions[l].sequence = favRecipe.RecipeInstructions[l].Sequence;
                        Recipe.directions[l].Value = favRecipe.RecipeInstructions[l].Instruction;
                    }
                }
            }
            response = Request.CreateResponse<weeksWeekDayMealRecipe>(HttpStatusCode.OK, Recipe);
            return response;
        }

        [HttpGet]
        [ActionName("GetCampFeed")]
        [ResponseType(typeof(BootcampDto.BootCampFeedResponse))]
        [Authorize]
        public HttpResponseMessage GetCampFeed(int id,int pageNumber,int PageSize)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            User user = userUow.Repository<User>().FindById(userId);

            BootCamp.DomainObjects.BootCamp.BootCamp bootcamp = uow.Repository<BootCamp.DomainObjects.BootCamp.BootCamp>().FindById(id);
            int recordCount = 0;
            List<BootCamp.DomainObjects.BootCamp.ActivityLog> activityLogs = null;
            HttpResponseMessage response = null;
            if (userId == bootcamp.CreatorId)
            {
                recordCount = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Include(o => o.BootCampUserDetail.BootCampUser)
                    .Include(o => o.BootCampUserDetail.BootCamp)
                    .Filter(o => o.BootCampUserDetail.BootCampId == id && 
                    ((o.LogType == LogType.Meal && o.ActivityStatus != ActivityStatus.Undefined) || (o.LogType == LogType.Workout)) && 
                    (o.LogPrivacyStatus == LogPrivacyStatus.LeaderOnly || 
                    o.LogPrivacyStatus == LogPrivacyStatus.Public || 
                    o.LogPrivacyStatus == LogPrivacyStatus.Undefined || 
                    o.BootCampUserDetail.UserId == userId))
                    .Get().Count();
                activityLogs = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Include(o => o.BootCampUserDetail.BootCampUser)
                    .Include(o => o.BootCampUserDetail.BootCamp)
                    .Filter(o => o.BootCampUserDetail.BootCampId == id &&
                   ((o.LogType == LogType.Meal && o.ActivityStatus != ActivityStatus.Undefined) || (o.LogType == LogType.Workout)) &&  
                    (o.LogPrivacyStatus == LogPrivacyStatus.LeaderOnly || o.LogPrivacyStatus == LogPrivacyStatus.Public || o.LogPrivacyStatus == LogPrivacyStatus.Undefined || o.BootCampUserDetail.UserId == userId))
                    .Get()
                    .OrderByDescending(c => c.Id)
                    //.OrderByDescending(c=>c.LogTime)
                    .Skip(pageNumber * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            else
            {
                recordCount = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Include(o => o.BootCampUserDetail.BootCampUser)
                    .Include(o => o.BootCampUserDetail.BootCamp)
                    .Filter(o => o.BootCampUserDetail.BootCampId == id &&
                    ((o.LogType == LogType.Meal && o.ActivityStatus != ActivityStatus.Undefined) || (o.LogType == LogType.Workout)) &&
                    (o.LogPrivacyStatus == LogPrivacyStatus.Public || o.LogPrivacyStatus == LogPrivacyStatus.Undefined || o.BootCampUserDetail.UserId == userId))
                    .Get().Count();
                activityLogs = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Include(o => o.BootCampUserDetail.BootCampUser)
                    .Include(o => o.BootCampUserDetail.BootCamp)
                    .Filter(o => o.BootCampUserDetail.BootCampId == id &&
                    ((o.LogType == LogType.Meal && o.ActivityStatus != ActivityStatus.Undefined) || (o.LogType == LogType.Workout)) &&
                    (o.LogPrivacyStatus == LogPrivacyStatus.Public || o.LogPrivacyStatus == LogPrivacyStatus.Undefined || o.BootCampUserDetail.UserId == userId))
                    .Get()
                    .OrderByDescending(c => c.Id)
                    //.OrderByDescending(c => c.LogTime)
                    .Skip(pageNumber * PageSize)
                    .Take(PageSize)
                    .ToList();
            }
            
            BootcampDto.BootCampFeedResponse BootCampFeedResponse = new BootcampDto.BootCampFeedResponse();
            BootCampFeedResponse.TotalRecords = recordCount;
            if (activityLogs != null && activityLogs.Count>0)
            {
                BootCampFeedResponse.Data = new BootcampDto.FeedData();
                
                BootCampFeedResponse.Data.Feed = new List<BootcampDto.FeedDto>(activityLogs.Count());
                activityLogs.ForEach(activityLog =>
                {
                    BootcampDto.FeedDto feedDto = new BootcampDto.FeedDto();
                    DateTime startDate = activityLog.BootCampUserDetail.BootCamp.StartDate;
                    DateTime activityDate = startDate.AddDays(activityLog.DayNo - 1);
                    DateTime currentUserDate = DateTime.UtcNow.AddMinutes(user.TimeZoneOffset);
                    if (activityLog.LogType == LogType.Workout && activityLog.ActivityStatus == ActivityStatus.Undefined && currentUserDate.Date > activityDate)
                    {
                        feedDto.UserImage = activityLog.BootCampUserDetail.BootCampUser.ProfilePicture;
                        feedDto.FirstName = activityLog.BootCampUserDetail.BootCampUser.FirstName;
                        feedDto.FullName = activityLog.BootCampUserDetail.BootCampUser.FullName;
                        feedDto.LastName = activityLog.BootCampUserDetail.BootCampUser.LastName;
                        feedDto.Text = "Missed workout";

                        //feedDto.Image = activityLog.Image;
                        feedDto.LogType = LogType.Workout;
                        feedDto.Status = ActivityStatus.Skipped;
                        feedDto.ActivityTime = activityLog.BootCampUserDetail.BootCamp.StartDate.AddDays(activityLog.DayNo - 1);
                        //feedDto.ActivityTime = activityLog.LogTime.AddMinutes(user.TimeZoneOffset);
                        //feedDto.MinutesAgo = DateTime.UtcNow.Subtract(activityLog.LogTime).TotalMinutes;
                        BootCampFeedResponse.Data.Feed.Add(feedDto);
                    }
                    else
                    {
                        feedDto.UserImage = activityLog.BootCampUserDetail.BootCampUser.ProfilePicture;
                        feedDto.FirstName = activityLog.BootCampUserDetail.BootCampUser.FirstName;
                        feedDto.FullName = activityLog.BootCampUserDetail.BootCampUser.FullName;
                        feedDto.LastName = activityLog.BootCampUserDetail.BootCampUser.LastName;
                        feedDto.Text = activityLog.Description;
                        if (activityLog.LogType == LogType.Meal && activityLog.MealType.HasValue)
                        {
                            feedDto.MealType = activityLog.MealType.Value;
                        }
                        feedDto.Image = activityLog.Image;
                        feedDto.LogType = activityLog.LogType;
                        feedDto.Status = activityLog.ActivityStatus;
                        if (activityLog.LogTime != DateTime.MinValue)
                        {
                            feedDto.ActivityTime = activityLog.LogTime.AddMinutes(user.TimeZoneOffset);
                        }
                        else
                        {
                            feedDto.ActivityTime = activityLog.BootCampUserDetail.BootCamp.StartDate.AddDays(activityLog.DayNo - 1);
                        }
                        feedDto.MinutesAgo = DateTime.UtcNow.Subtract(activityLog.LogTime).TotalMinutes;
                        BootCampFeedResponse.Data.Feed.Add(feedDto);
                    }
                });
            }

            response = Request.CreateResponse<BootcampDto.BootCampFeedResponse>(HttpStatusCode.OK, BootCampFeedResponse);


            return response;
        }

        [HttpGet]
        [ActionName("GetBootCamp")]
        [ResponseType(typeof(BootcampDto.BootCampDetail))]
        [Authorize]
        public HttpResponseMessage GetBootCamp(int id)
        {
            HttpResponseMessage response = null;
            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>()
                .Query()
                .Include(p => p.BootCampInvitedUsers.Select(c=>c.User))
                .Include(p => p.Owner)
                .Include(p => p.BootCampUserDetails.Select(c=>c.BootCampUser))
                .Filter(o => o.Id == id)
                .Get()
                .FirstOrDefault();
            if (bootCamp.BootCampStatus == BootCampStatus.Active)
            {
                BootcampDto.BootCampDetail bootCampDetail = new BootcampDto.BootCampDetail
                {
                    Id = bootCamp.Id,
                    About = bootCamp.About,
                    StartDate = bootCamp.StartDate,
                    EndDate = bootCamp.EndDate,
                    IsPrivate = bootCamp.IsPrivate,
                    ImageUrl = bootCamp.BootcampImage,
                    MemebersLimit = bootCamp.MaxMembers,
                    Name = bootCamp.Name,
                    Status = bootCamp.BootCampStatus,
                    RegisteredMemebers = bootCamp.SignedUp
                };

                if (bootCamp.BootCampInvitedUsers != null && bootCamp.BootCampInvitedUsers.Count > 0)
                {
                    bootCampDetail.InvitedGroupies = new List<BootcampDto.BootCampUser>(bootCamp.BootCampInvitedUsers.Count);
                    bootCamp.BootCampInvitedUsers.ToList().ForEach((invitedUser) =>
                    {
                        bootCampDetail.InvitedGroupies.Add(new BootcampDto.BootCampUser
                        {
                            Id = invitedUser.UserId,
                            FirstName = invitedUser.User.FirstName,
                            LastName = invitedUser.User.LastName,
                            ImageUrl = invitedUser.User.ProfilePicture
                        });
                    });
                }
                bootCamp.BootCampUserDetails = bootCamp.BootCampUserDetails.Where(o => o.DroppedOut == false).ToList();
                if (bootCamp.BootCampUserDetails != null && bootCamp.BootCampUserDetails.Count > 0)
                {
                    bootCampDetail.Groupies = new List<BootcampDto.BootCampUser>(bootCamp.BootCampUserDetails.Count);
                    bootCamp.BootCampUserDetails.ToList().ForEach((bootcampUser) =>
                    {
                        bootCampDetail.Groupies.Add(new BootcampDto.BootCampUser
                        {
                            Id = bootcampUser.UserId,
                            FirstName = bootcampUser.BootCampUser.FirstName,
                            LastName = bootcampUser.BootCampUser.LastName,
                            ImageUrl = bootcampUser.BootCampUser.ProfilePicture
                        });
                    });
                }
                if (bootCamp.Owner != null)
                {
                    bootCampDetail.Owner = new Models.DTO.BootCampUser
                    {
                        Id = bootCamp.Owner.Id,
                        FirstName = bootCamp.Owner.FirstName,
                        LastName = bootCamp.Owner.LastName,
                        ImageUrl = bootCamp.Owner.ProfilePicture
                    };
                }
                response = Request.CreateResponse<BootcampDto.BootCampDetail>(HttpStatusCode.OK, bootCampDetail);
            }
            else
            {
                response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest, new BootcampDto.ErrorResponse("This bootcamp has been deleted by owner."));
            }
            return response;
        }

        [HttpPost]
        [ActionName("GetBootCamps")]
        [ResponseType(typeof(BootcampDto.BootcampListDto))]
        [Authorize]
        public HttpResponseMessage GetBootCamps(BootcampDto.BootcampsRequest request)
        {
            List<BootcampDto.Bootcamp> bootcampsDto = new List<BootcampDto.Bootcamp>();

            Expression<Func<Bootcamp.BootCamp, bool>> filter = null;

            if (string.IsNullOrEmpty(request.BootCampName))
            {
                filter = o => o.StartDate > DateTime.Today && o.IsPrivate == false && o.BootCampStatus == BootCampStatus.Active;
            }
            else
            {
                filter = o => o.StartDate > DateTime.Today && o.IsPrivate == false && o.BootCampStatus == BootCampStatus.Active
                && o.Name.Trim().ToLower().Contains(request.BootCampName.Trim().ToLower());
            }

            int totalCount = uow.Repository<Bootcamp.BootCamp>()
                .Query()
                .Filter(filter)
                .Get().Count();

            List<Bootcamp.BootCamp> bootCamps = uow.Repository<Bootcamp.BootCamp>()
                .Query()
                .Filter(filter)
                .Get()
                .OrderBy(o => o.StartDate)
                .Take(request.PageSize)
                .Skip(request.PageSize * request.PageNumber)
                .ToList();

            bootCamps.ForEach((bootcamp) =>
            {
                BootcampDto.Bootcamp bootcampDto = new BootcampDto.Bootcamp();
                bootcampDto.Id = bootcamp.Id;
                bootcampDto.Name = bootcamp.Name;
                bootcampDto.About = bootcamp.About;
                bootcampDto.ImageUrl = bootcamp.BootcampImage; 
                bootcampDto.MemebersLimit = 15;
                bootcampDto.Status = bootcamp.BootCampStatus;
                bootcampDto.RegisteredMemebers = bootcamp.SignedUp;
                bootcampDto.StartDate = bootcamp.StartDate;
                bootcampsDto.Add(bootcampDto);
            });
            BootcampDto.BootcampListDto bootCampList = new BootcampDto.BootcampListDto
            {
                TotalRecord = totalCount,
                BootCamps = bootcampsDto
            };
            HttpResponseMessage response = Request.CreateResponse<BootcampDto.BootcampListDto>(HttpStatusCode.OK, bootCampList);
            return response;
        }

        [HttpPost]
        [Authorize]
        [ActionName("EditBootcamp")]
        public HttpResponseMessage EditBootCamp(BootcampDto.EditBootCamp request)
        {
            HttpResponseMessage response = null;
            try
            {
                ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

                Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
                int userId = int.Parse(claim.Value);

                Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(request.Id);

                if (bootCamp.CreatorId == userId)
                {
                    bootCamp.Name = request.Name;
                    bootCamp.About = request.About;
                    bootCamp.BootcampImage = request.ImageUrl;
                    bootCamp.State = ObjectInterfaces.ObjectState.Modified;
                    bootCamp.UpdatedOn = DateTime.Now;
                    uow.Repository<Bootcamp.BootCamp>().Update(bootCamp);
                    uow.Save();
                    response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse("Bootcamp Updated"));
                    return response;
                }
                else
                {
                    response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest, new BootcampDto.ErrorResponse("You are not allowed to edit this bootcamp. You are not the owner."));
                    return response;
                }
            }
            catch(Exception ex)
            {
                response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.InternalServerError,new BootcampDto.ErrorResponse("An Error has occured."));
                return response;
            }
        }

        [HttpPost]
        [Authorize]
        [ActionName("JoinBootcamp")]

        public HttpResponseMessage JoinBootCamp(BootcampDto.JoinBootCampRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            HttpResponseMessage response = null;
            try
            {
                int currentBootCampCount = uow.Repository<Bootcamp.BootCampUserDetail>()
                        .Query()
                        .Filter(o => o.UserId == userId && o.BootCamp.EndDate > DateTime.Now && o.BootCamp.BootCampStatus == BootCampStatus.Active && o.DroppedOut == false)
                        .Get()
                        .ToList().Count();
                if(currentBootCampCount > 0)
                {
                    return Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest, new BootcampDto.ErrorResponse("You have already joined another bootcamp."));
                }

                Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(request.Id);
                
                if (bootCamp.MaxMembers > bootCamp.SignedUp)
                {
                    Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                        .Query()
                        .Filter(o => o.UserId == userId && o.BootCampId==request.Id && o.BootCamp.EndDate > DateTime.Now)
                        .Get()
                        .FirstOrDefault();

                    if (bootCampUserDetail == null)
                    {
                        bootCampUserDetail = new Bootcamp.BootCampUserDetail
                        {
                            DroppedOut = false,
                            BootCampId = bootCamp.Id,
                            BootCamp = bootCamp,
                            UserId = userId,
                            CreatedOn = DateTime.Now,
                            UpdatedOn = DateTime.Now,
                            JoinDate = DateTime.Now,
                            State = ObjectInterfaces.ObjectState.Added,
                        };
                        bootCamp.SignedUp++;
                        bootCamp.UpdatedOn = DateTime.Now;
                        bootCamp.State = ObjectInterfaces.ObjectState.Modified;
                        if(bootCamp.BootCampUserDetails == null)
                        {
                            bootCamp.BootCampUserDetails = new List<Bootcamp.BootCampUserDetail>();
                        }
                        bootCamp.BootCampUserDetails.Add(bootCampUserDetail);
                        Bootcamp.BootCampInvitedUsers bootCampInvitedUser = uow.Repository<Bootcamp.BootCampInvitedUsers>()
                            .Query()
                            .Filter(o => o.UserId == userId && o.BootCampId == bootCamp.Id)
                            .Get()
                            .FirstOrDefault();

                        if(bootCampInvitedUser != null)
                        {
                            uow.Repository<Bootcamp.BootCampInvitedUsers>().Delete(bootCampInvitedUser.Id);
                        }
                        uow.Repository<Bootcamp.BootCamp>().Update(bootCamp);
                        uow.Save();

                        List<UserNotification> userNotifications = notificationUow.Repository<UserNotification>()
                            .Query()
                            .Filter(o => o.ForUserId == userId && (o.NotificationForId.HasValue == true && o.NotificationForId.Value != request.Id))
                            .Get()
                            .ToList();

                        userNotifications.ForEach((userNotification) =>
                        {
                            userNotification.State = ObjectInterfaces.ObjectState.Deleted;
                            notificationUow.Repository<UserNotification>().Delete(userNotification);
                        });
                        notificationUow.Save();

                        response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK,new BootcampDto.SuccessResponse("Joined bootcamp"));
                    }
                    else if (bootCampUserDetail != null && bootCampUserDetail.DroppedOut == true)
                    {
                        bootCampUserDetail.DroppedOut = false;
                        bootCampUserDetail.UpdatedOn = DateTime.Now;
                        bootCampUserDetail.State = ObjectInterfaces.ObjectState.Modified;
                        bootCamp.SignedUp++;
                        bootCamp.DroppedOff--;
                        bootCamp.UpdatedOn = DateTime.Now;
                        bootCamp.State = ObjectInterfaces.ObjectState.Modified;
                        uow.Repository<Bootcamp.BootCamp>().Update(bootCamp);
                        uow.Save();
                        response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse("Rejoined bootcamp"));
                    }
                    else
                    {
                        response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest, new BootcampDto.ErrorResponse("You have already joined the bootcamp."));
                    }
                }
                else
                {
                    response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest,new BootcampDto.ErrorResponse("Members cannot exceed 15."));
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.InternalServerError, new BootcampDto.ErrorResponse(ex.Message));
            }
            return response;
        }
        [HttpPost]
        [Authorize]
        [ActionName("ReportBootcamp")]
        public HttpResponseMessage ReportBootcamp(BootcampDto.ReportBootcampRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>()
                    .Query()
                    .Include(t => t.BootCampReports)
                    .Filter(c => c.Id == request.BootCampId)
                    .Get()
                    .FirstOrDefault();

            if(bootCamp != null)
            {
                if(bootCamp.BootCampReports != null && bootCamp.BootCampReports.Where(o=>o.UserId == userId).Count() > 0)
                {                    return Request.CreateResponse<string>(HttpStatusCode.BadRequest, "You have already reported on this boot camp.");
                }
                if (bootCamp.BootCampReports == null)
                {
                    bootCamp.BootCampReports = new List<Bootcamp.BootCampReport>();
                }
                bootCamp.BootCampReports.Add(new Bootcamp.BootCampReport() { UserId = userId,BootCampId = bootCamp.Id,State = ObjectInterfaces.ObjectState.Added });
                if(bootCamp.BootCampReports.Count() > 2)
                {
                    bootCamp.BootCampStatus = BootCampStatus.Banned;
                }
                bootCamp.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<Bootcamp.BootCamp>().Update(bootCamp);
                uow.Save();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpPost]
        [Authorize]
        [ActionName("UploadImage")]
        public async Task<BootcampDto.FileUploadResponse> UploadImage()
        {
            var uploadPath = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["BootImages"]);

            var multipartFormDataStreamProvider = new UploadMultipartFormProvider(uploadPath);

            // Read the MIME multipart asynchronously 
            await Request.Content.ReadAsMultipartAsync(multipartFormDataStreamProvider);

            string _localFileName = multipartFormDataStreamProvider
                .FileData.Select(multiPartData => multiPartData.LocalFileName).FirstOrDefault();

            // Create response
            return new BootcampDto.FileUploadResponse
            {
                Url = (ConfigurationManager.AppSettings["BootImages"] + "//" + Path.GetFileName(_localFileName)).ToAbsoluteUrl(),
                FileName = Path.GetFileName(_localFileName),
                FileLength = new FileInfo(_localFileName).Length
            };
        }
        
        [HttpPost]
        [ActionName("DeleteBootCamp")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        public HttpResponseMessage DeleteBootCamp(BootcampDto.DeleteBootCampRequest request)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>()
                    .Query()
                    .Include(t=>t.BootCampUserDetails.Select(u=>u.BootCampUser))
                    .Filter(c => c.Id == request.BootCampId && c.Owner.Id == userId)
                    .Get()
                    .FirstOrDefault();

            if (bootCamp == null)
            {
                response = Request.CreateResponse<string>(HttpStatusCode.Forbidden, "You are not the owner of the bootcamp");
            }
            else if(bootCamp.EndDate.Date < System.DateTime.UtcNow.Date)
            {
                response = Request.CreateResponse<string>(HttpStatusCode.Forbidden, "Unable to delete a completed bootcamp");
            }
            else
            {
                bootCamp.BootCampStatus = BootCampStatus.Deleted;
                bootCamp.UpdatedOn = DateTime.UtcNow;
                bootCamp.State = ObjectInterfaces.ObjectState.Modified;
                bootCamp.BootCampUserDetails.ToList().ForEach(bootCampUserDetail =>
                    {
                        if (bootCampUserDetail.DroppedOut == false)
                        {
                            if (bootCampUserDetail.UserId != userId)
                            {
                                UserNotification notifyUser = new UserNotification
                                {
                                    ForUserId = bootCampUserDetail.BootCampUser.Id,
                                    NotificationType = NotificationType.BootCampDeleted,
                                    NotificationForId = bootCamp.Id,
                                    State = ObjectInterfaces.ObjectState.Added,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedOn = DateTime.UtcNow,
                                    TextLine1 = bootCamp.Name + " has been deleted by its owner.",
                                    Title = "Bootcamp has been deleted",
                                };
                                notificationUow.Repository<UserNotification>().InsertGraph(notifyUser);
                            }
                            bootCampUserDetail.DroppedOut = true;
                            bootCampUserDetail.UpdatedOn = DateTime.UtcNow;
                            bootCampUserDetail.State = ObjectInterfaces.ObjectState.Modified;
                        }
                    });
                uow.Repository<Bootcamp.BootCamp>().Update(bootCamp);
                uow.Save();
                notificationUow.Save();
                response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse("Bootcamp deleted."));
            }
            return response;
        }
        [HttpGet]
        [ActionName("GetTodaysMeal")]
        [ResponseType(typeof(weeksWeekDay))]
        [Authorize]
        public HttpResponseMessage GetTodaysMeal(int bootCampId)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(bootCampId);

            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>()
                .Query()
                .Include(o => o.UserNotificationSetting)
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            Bootcamp.BootCampUserDetail bootCampUserdetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Filter(o => o.UserId == userId && o.BootCampId == bootCamp.Id)
                .Get()
                .FirstOrDefault();

            DateTime userDateTime = DateTime.UtcNow.AddMinutes(bootCampUser.TimeZoneOffset);

            int DayNo = ((userDateTime - bootCamp.StartDate).Days) + 1;
            int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
            int weekDayNo = DayNo - (7 * (weekNo - 1));
            
            List<BootCamp.DomainObjects.BootCamp.ActivityLog> activityLogs = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserId == bootCampUserdetail.Id && o.DayNo == DayNo && o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.LogType == LogType.Meal)
                    .Get().ToList();

            if (activityLogs != null)
            {
                weeksWeekDay day = new weeksWeekDay();
                day.meal = new weeksWeekDayMeal[activityLogs.Count];
                int mealCount = 0;
                activityLogs.ForEach(activityLog =>
                {
                    List<int> favRecipes = uow.Repository<Bootcamp.FavoriteRecipe>().Query()
                        .Filter(o => o.UserId == userId && o.Favorite == MarkAs.Favorite).Get().Select(s => s.RecipeId).ToList();

                    List<Bootcamp.Meal> dayMealPlans = uow.Repository<Bootcamp.Meal>()
                        .Query()
                        .Include(c => c.Recipe.RecipeIngredients)
                        .Include(c => c.Recipe.RecipeInstructions)
                        .Filter(o => o.WeekNumber == weekNo && o.DayOfWeek == weekDayNo && o.DietTypeId == bootCampUser.MealPlan && o.MealType == activityLog.MealType.Value)
                        .Get()
                        .ToList();

                    weeksWeekDayMeal meal = new weeksWeekDayMeal();
                    meal.type = activityLog.MealType.Value.ToString();
                    meal.ActivtyLogId = activityLog.Id;
                    meal.ActivityStatus = activityLog.ActivityStatus;
                    meal.recipe = new weeksWeekDayMealRecipe[dayMealPlans.Count];
                    int count = 0;
                    dayMealPlans.ForEach(dayMealPlan =>
                    {
                        meal.recipe[count] = new weeksWeekDayMealRecipe();
                        if (dayMealPlan.Recipe.Cooktime.HasValue)
                            meal.recipe[count].cooktime = dayMealPlan.Recipe.Cooktime.Value;

                        meal.recipe[count] = new weeksWeekDayMealRecipe();
                        meal.recipe[count].Id = dayMealPlan.Recipe.Id;
                        meal.recipe[count].image = dayMealPlan.Recipe.Image;
                        meal.recipe[count].name = dayMealPlan.Recipe.Name;
                        if (dayMealPlan.Recipe.Preptime.HasValue)
                            meal.recipe[count].preptime = dayMealPlan.Recipe.Preptime.Value;
                        if (dayMealPlan.Recipe.Cooktime.HasValue)
                            meal.recipe[count].cooktime = dayMealPlan.Recipe.Cooktime.Value;
                        if (dayMealPlan.Recipe.Readintime.HasValue)
                            meal.recipe[count].readintime = dayMealPlan.Recipe.Readintime.Value;
                        meal.recipe[count].IsFavorite = favRecipes.Contains(dayMealPlan.Recipe.Id) == true ? true : false;

                        if (dayMealPlan.Recipe.RecipeIngredients != null && dayMealPlan.Recipe.RecipeIngredients.Count > 0)
                        {
                            meal.recipe[count].ingredients = new weeksWeekDayMealRecipeIngredient[dayMealPlan.Recipe.RecipeIngredients.Count];
                            for (int l = 0; l < meal.recipe[count].ingredients.Length; l++)
                            {
                                meal.recipe[count].ingredients[l] = new weeksWeekDayMealRecipeIngredient();
                                meal.recipe[count].ingredients[l].name = dayMealPlan.Recipe.RecipeIngredients[l].Ingredient;
                            }
                        }
                        if (dayMealPlan.Recipe.RecipeInstructions != null && dayMealPlan.Recipe.RecipeInstructions.Count > 0)
                        {
                            meal.recipe[count].directions = new weeksWeekDayMealRecipeDirection[dayMealPlan.Recipe.RecipeInstructions.Count];
                            for (int l = 0; l < meal.recipe[count].directions.Length; l++)
                            {
                                meal.recipe[count].directions[l] = new weeksWeekDayMealRecipeDirection();
                                meal.recipe[count].directions[l].sequence = dayMealPlan.Recipe.RecipeInstructions[l].Sequence;
                                meal.recipe[count].directions[l].Value = dayMealPlan.Recipe.RecipeInstructions[l].Instruction;
                            }
                        }
                        count++;
                        day.meal[mealCount] = meal;
                        
                    });
                    mealCount++;
                });
                response = Request.CreateResponse<weeksWeekDay>(HttpStatusCode.OK, day);
            }
            else
            {
                response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.NotFound, new BootcampDto.ErrorResponse("Not found"));
            }
            return response;
        }
        [HttpGet]
        [ActionName("GetMealDetail")]
        [ResponseType(typeof(weeksWeekDayMeal))]
        [Authorize]
        public HttpResponseMessage GetMealDetail(int bootCampId)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(bootCampId);

            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>()
                .Query()
                .Include(o=>o.UserNotificationSetting)
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            Bootcamp.BootCampUserDetail bootCampUserdetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Filter(o => o.UserId == userId && o.BootCampId == bootCamp.Id)
                .Get()
                .FirstOrDefault();

            DateTime userDateTime = DateTime.UtcNow.AddMinutes(bootCampUser.TimeZoneOffset);

            int DayNo = ((userDateTime - bootCamp.StartDate).Days) + 1;
            int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
            int weekDayNo = DayNo - (7 * (weekNo - 1));

            TimeSpan breakfastStartTime = new TimeSpan(6, 0, 0);
            TimeSpan breakfastEndTime = new TimeSpan(9, 0, 0);

            TimeSpan snack1StartTime = new TimeSpan(10, 0, 0);
            TimeSpan snack1EndTime = new TimeSpan(12, 0, 0);

            TimeSpan lunchStartTime = new TimeSpan(13, 0, 0);
            TimeSpan lunchEndTime = new TimeSpan(15, 0, 0);

            TimeSpan snack2StartTime = new TimeSpan(15, 1, 0);
            TimeSpan snack2EndTime = new TimeSpan(17, 0, 0);

            TimeSpan dinnerStartTime = new TimeSpan(18, 0, 0);
            TimeSpan dinnerEndTime = new TimeSpan(20, 0, 0);

            MealType mealType = MealType.Undefined;
            if (userDateTime.TimeOfDay >= breakfastStartTime && userDateTime.TimeOfDay <= breakfastEndTime)
            {
                mealType = MealType.Breakfast;
            }
            else if (userDateTime.TimeOfDay >= snack1StartTime && userDateTime.TimeOfDay <= snack1EndTime)
            {
                mealType = MealType.Snack1;
            }
            else if (userDateTime.TimeOfDay >= lunchStartTime && userDateTime.TimeOfDay <= lunchEndTime)
            {
                mealType = MealType.Lunch;
            }
            else if (userDateTime.TimeOfDay >= snack2StartTime && userDateTime.TimeOfDay <= snack2EndTime)
            {
                mealType = MealType.Snack2;
            }
            else if (userDateTime.TimeOfDay >= dinnerStartTime && userDateTime.TimeOfDay <= dinnerEndTime)
            {
                mealType = MealType.Dinner;
            }
            BootCamp.DomainObjects.BootCamp.ActivityLog activityLog = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserId == bootCampUserdetail.Id && o.DayNo == DayNo && o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.MealType == mealType && o.LogType == LogType.Meal)
                    .Get().FirstOrDefault();

            if (activityLog != null)
            {
                List<int> favRecipes = uow.Repository<Bootcamp.FavoriteRecipe>().Query()
                    .Filter(o => o.UserId == userId && o.Favorite == MarkAs.Favorite).Get().Select(s => s.RecipeId).ToList();

                List<Bootcamp.Meal> dayMealPlans = uow.Repository<Bootcamp.Meal>()
                    .Query()
                    .Include(c => c.Recipe.RecipeIngredients)
                    .Include(c => c.Recipe.RecipeInstructions)
                    .Filter(o => o.WeekNumber == weekNo && o.DayOfWeek == weekDayNo && o.DietTypeId == bootCampUser.MealPlan && o.MealType == mealType)
                    .Get()
                    .ToList();

                weeksWeekDayMeal meal = new weeksWeekDayMeal();
                meal.type = mealType.ToString();
                meal.ActivtyLogId = activityLog.Id;
                meal.ActivityStatus = activityLog.ActivityStatus;
                meal.recipe = new weeksWeekDayMealRecipe[dayMealPlans.Count];
                int count = 0;
                dayMealPlans.ForEach(dayMealPlan =>
                {
                    meal.recipe[count] = new weeksWeekDayMealRecipe();
                    if(dayMealPlan.Recipe.Cooktime.HasValue)
                    meal.recipe[count].cooktime = dayMealPlan.Recipe.Cooktime.Value;
                   
                    meal.recipe[count] = new weeksWeekDayMealRecipe();
                    meal.recipe[count].Id = dayMealPlan.Recipe.Id;
                    meal.recipe[count].image = dayMealPlan.Recipe.Image;
                    meal.recipe[count].name = dayMealPlan.Recipe.Name;
                    if (dayMealPlan.Recipe.Preptime.HasValue)
                        meal.recipe[count].preptime = dayMealPlan.Recipe.Preptime.Value;
                    if (dayMealPlan.Recipe.Cooktime.HasValue)
                        meal.recipe[count].cooktime = dayMealPlan.Recipe.Cooktime.Value;
                    if (dayMealPlan.Recipe.Readintime.HasValue)
                        meal.recipe[count].readintime = dayMealPlan.Recipe.Readintime.Value;
                    meal.recipe[count].IsFavorite = favRecipes.Contains(dayMealPlan.Recipe.Id) == true ? true : false;
                    
                    if (dayMealPlan.Recipe.RecipeIngredients != null && dayMealPlan.Recipe.RecipeIngredients.Count > 0)
                    {
                        meal.recipe[count].ingredients = new weeksWeekDayMealRecipeIngredient[dayMealPlan.Recipe.RecipeIngredients.Count];
                        for (int l = 0; l < meal.recipe[count].ingredients.Length; l++)
                        {
                            meal.recipe[count].ingredients[l] = new weeksWeekDayMealRecipeIngredient();
                            meal.recipe[count].ingredients[l].name = dayMealPlan.Recipe.RecipeIngredients[l].Ingredient;
                        }
                    }
                    if (dayMealPlan.Recipe.RecipeInstructions != null && dayMealPlan.Recipe.RecipeInstructions.Count > 0)
                    {
                        meal.recipe[count].directions = new weeksWeekDayMealRecipeDirection[dayMealPlan.Recipe.RecipeInstructions.Count];
                        for (int l = 0; l < meal.recipe[count].directions.Length; l++)
                        {
                            meal.recipe[count].directions[l] = new weeksWeekDayMealRecipeDirection();
                            meal.recipe[count].directions[l].sequence = dayMealPlan.Recipe.RecipeInstructions[l].Sequence;
                            meal.recipe[count].directions[l].Value = dayMealPlan.Recipe.RecipeInstructions[l].Instruction;
                        }
                    }
                    count++;
                });
                response = Request.CreateResponse<weeksWeekDayMeal>(HttpStatusCode.OK, meal);
            }
            else
            {
                response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.NotFound, new BootcampDto.ErrorResponse("Not found"));
            }
            return response;
        }
        [HttpGet]
        [ActionName("GetMealPlan")]
        [ResponseType(typeof(WeekData))]
        [Authorize]
        public HttpResponseMessage GetMealPlan(int bootCampId,int weekNo)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUser BootCampUser = uow.Repository<Bootcamp.BootCampUser>().FindById(userId);

            List<int> favRecipes = uow.Repository<Bootcamp.FavoriteRecipe>().Query()
                .Filter(o => o.UserId == userId && o.Favorite == MarkAs.Favorite).Get().Select(s=>s.RecipeId).ToList();

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(bootCampId);

            DateTime startDate = bootCamp.StartDate.AddDays(7*(weekNo - 1));
            
            List<Bootcamp.Meal> weeklyMealPlans = uow.Repository<Bootcamp.Meal>()
                .Query()
                .Include(c=>c.Recipe.RecipeIngredients)
                .Include(c => c.Recipe.RecipeInstructions)
                .Filter(o => o.WeekNumber == weekNo && o.DietTypeId == BootCampUser.MealPlan)
                .Get()
                .ToList();

            List<BootCamp.DomainObjects.BootCamp.ActivityLog> activityLogs = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserDetail.UserId == userId && o.WeekNo == weekNo && o.BootCampUserDetail.BootCampId == bootCampId 
                    && o.LogType== LogType.Meal)
                    .Get().ToList();

            weeksWeek week = new weeksWeek();

            int numDays = weeklyMealPlans.Select(m => m.DayOfWeek).Max();

            week.day = new weeksWeekDay[numDays];
            week.number = weekNo;

            for (int i=0;i < numDays; i++)
            {
                List<Bootcamp.Meal> dailyMealPlans = weeklyMealPlans.Where(o => o.DayOfWeek == (i + 1)).OrderBy(o=>o.MealSequence).ToList();
                week.day[i] = new weeksWeekDay();
                week.day[i].Day = startDate;
                week.day[i].name = startDate.DayOfWeek.ToString();

                int DayNo = ((startDate - bootCamp.StartDate).Days) + 1;
                int tempWeekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
                int weekDayNo = DayNo - (7 * (weekNo - 1));

                startDate = startDate.AddDays(1);
                week.day[i].sequence = i + 1;
                week.day[i].meal = new weeksWeekDayMeal[dailyMealPlans.Select(o => o.MealSequence).Distinct().Count()];

                int maxCount = dailyMealPlans.Select(o => o.MealSequence).Max();

                for(int j=0 ;j< maxCount ;j++)
                {
                    List<Bootcamp.Meal> dailyMealPlan = dailyMealPlans.Where(o => o.MealSequence == (j + 1)).ToList();
                    week.day[i].meal[j] = new weeksWeekDayMeal();
                    week.day[i].meal[j].sequence = j + 1;
                    MealType mealType = dailyMealPlan.Select(o => o.MealType).First();
                    week.day[i].meal[j].type = mealType.ToString();
                    week.day[i].meal[j].ActivityStatus = ActivityStatus.Undefined;
                    week.day[i].meal[j].ActivtyLogId = 0;
                    if (activityLogs != null && activityLogs.Count() > 0)
                    {
                        BootCamp.DomainObjects.BootCamp.ActivityLog log = activityLogs.Where(o => o.MealType == mealType 
                        && o.DayNo == DayNo && o.WeekNo == tempWeekNo && o.WeekDayNo == weekDayNo)
                        .FirstOrDefault();
                        if (log != null)
                        {
                            week.day[i].meal[j].ActivityStatus = log.ActivityStatus;
                            week.day[i].meal[j].ActivtyLogId = log.Id;
                        }
                    }

                    week.day[i].meal[j].recipe = new weeksWeekDayMealRecipe[dailyMealPlan.Count()];
                    for (int k = 0; k < week.day[i].meal[j].recipe.Length; k++)
                    {
                        week.day[i].meal[j].recipe[k] = new weeksWeekDayMealRecipe();
                        week.day[i].meal[j].recipe[k].Id = dailyMealPlan[k].Recipe.Id;
                        week.day[i].meal[j].recipe[k].image = dailyMealPlan[k].Recipe.Image;
                        week.day[i].meal[j].recipe[k].name = dailyMealPlan[k].Recipe.Name;
                        if(dailyMealPlan[k].Recipe.Preptime.HasValue)
                        week.day[i].meal[j].recipe[k].preptime = dailyMealPlan[k].Recipe.Preptime.Value;
                        if (dailyMealPlan[k].Recipe.Cooktime.HasValue)
                            week.day[i].meal[j].recipe[k].cooktime = dailyMealPlan[k].Recipe.Cooktime.Value;
                        week.day[i].meal[j].recipe[k].IsFavorite = favRecipes.Contains(dailyMealPlan[k].Recipe.Id) == true ? true : false;
                        if (dailyMealPlan[k].Recipe.Readintime.HasValue)
                            week.day[i].meal[j].recipe[k].readintime = dailyMealPlan[k].Recipe.Readintime.Value;
                        if (dailyMealPlan[k].Recipe.RecipeIngredients != null && dailyMealPlan[k].Recipe.RecipeIngredients.Count > 0)
                        {
                            week.day[i].meal[j].recipe[k].ingredients = new weeksWeekDayMealRecipeIngredient[dailyMealPlan[k].Recipe.RecipeIngredients.Count];
                            for(int l = 0;l< week.day[i].meal[j].recipe[k].ingredients.Length;l++)
                            {
                                week.day[i].meal[j].recipe[k].ingredients[l] = new weeksWeekDayMealRecipeIngredient();
                                week.day[i].meal[j].recipe[k].ingredients[l].name = dailyMealPlan[k].Recipe.RecipeIngredients[l].Ingredient;
                            }
                        }
                        if (dailyMealPlan[k].Recipe.RecipeInstructions != null && dailyMealPlan[k].Recipe.RecipeInstructions.Count > 0)
                        {
                            week.day[i].meal[j].recipe[k].directions = new weeksWeekDayMealRecipeDirection[dailyMealPlan[k].Recipe.RecipeInstructions.Count];
                            for (int l = 0; l < week.day[i].meal[j].recipe[k].directions.Length; l++)
                            {
                                week.day[i].meal[j].recipe[k].directions[l] = new weeksWeekDayMealRecipeDirection();
                                week.day[i].meal[j].recipe[k].directions[l].sequence = dailyMealPlan[k].Recipe.RecipeInstructions[l].Sequence;
                                week.day[i].meal[j].recipe[k].directions[l].Value = dailyMealPlan[k].Recipe.RecipeInstructions[l].Instruction;
                            }
                        }
                    }
                }
            }
            WeekData weekdata = new WeekData { TotalWeeks = 6, Week = week,CurrentWeek= weekNo };
            response = Request.CreateResponse<WeekData>(HttpStatusCode.OK, weekdata);
            return response;
        }
        [HttpGet]
        [ActionName("GetExercisePlan")]
        [ResponseType(typeof(Models.Exercise.WeekData))]
        [Authorize]
        public HttpResponseMessage GetExercisePlan(int bootCampId,int weekNo)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUser BootCampUser = uow.Repository<Bootcamp.BootCampUser>().FindById(userId);

            WorkoutLevel level = WorkoutLevel.Beginner;
            if(BootCampUser.WorkoutLevel != WorkoutLevel.None)
            {
                level = BootCampUser.WorkoutLevel;
            }

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>()
                .Query()
                .Filter(o => o.Id == bootCampId)
                .Get()
                .FirstOrDefault();
            DateTime startDate = bootCamp.StartDate;
            startDate = startDate.AddDays((weekNo - 1) * 7);
            List<Bootcamp.Workout> workouts = uow.Repository<Bootcamp.Workout>()
                .Query()
                .Include(t => t.Exercises)
                .Filter(o => o.WeekNo == weekNo && o.WorkoutType == WorkoutType.Home && o.WorkoutLevel == level)
                .Get()
                .ToList();
            Models.Exercise.WeekData WeekData = new Models.Exercise.WeekData();
            WeekData.CurrentWeek = weekNo;
            WeekData.TotalWeeks = 6;


            List<BootCamp.DomainObjects.BootCamp.ActivityLog> activityLogs = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserDetail.UserId == userId && o.WeekNo == weekNo && o.BootCampUserDetail.BootCampId == bootCampId
                    && o.LogType == LogType.Workout)
                    .Get().ToList();

            if (workouts != null)
            {
                int i = 0;
                WeekData.Week = new Models.Exercise.weeksWeek();
                WeekData.Week.number = weekNo;

                WeekData.Week.day = new Models.Exercise.weeksWeekDay[7];
                workouts.ForEach(paramWorkout =>
                {
                    if (paramWorkout.Exercises != null)
                    {
                        paramWorkout.Exercises = paramWorkout.Exercises.OrderBy(o => o.Id).ToList();
                    }
                    Bootcamp.Workout workout = paramWorkout;

                    WeekData.Week.day[i] = new Models.Exercise.weeksWeekDay();
                    WeekData.Week.day[i].Day = startDate;
                    WeekData.Week.day[i].name = WeekData.Week.day[i].Day.DayOfWeek.ToString();
                        
                    WeekData.Week.day[i].workouts = new Models.Exercise.weeksWeekDayWorkouts();

                    WeekData.Week.day[i].workouts.ActivityStatus = ActivityStatus.Undefined;
                    WeekData.Week.day[i].workouts.ActivityLogId = 0;

                    if (activityLogs != null)
                    {
                        BootCamp.DomainObjects.BootCamp.ActivityLog log = activityLogs.Where(o => o.WeekDayNo == workout.WeekDayNo && o.WeekNo == workout.WeekNo).FirstOrDefault();
                        if(log != null)
                        {
                            WeekData.Week.day[i].workouts.ActivityStatus = log.ActivityStatus;
                            WeekData.Week.day[i].workouts.ActivityLogId = log.Id;
                            if(log.WorkoutId.HasValue)
                            {
                                workout = uow.Repository<Bootcamp.Workout>()
                                    .Query()
                                    .Include(t => t.Exercises)
                                    .Filter(o => o.Id == log.WorkoutId.Value)
                                    .Get()
                                    .FirstOrDefault();
                                if (workout != null)
                                {
                                    WeekData.Week.day[i].workouts.WorkoutLevel = workout.WorkoutLevel;
                                    WeekData.Week.day[i].workouts.WorkoutType = workout.WorkoutType;
                                }
                                else
                                {
                                    WeekData.Week.day[i].workouts.WorkoutLevel = WorkoutLevel.None;
                                    WeekData.Week.day[i].workouts.WorkoutType = WorkoutType.NotSet;
                                }
                            }
                            else
                            {
                                WeekData.Week.day[i].workouts.WorkoutLevel = WorkoutLevel.None;
                                WeekData.Week.day[i].workouts.WorkoutType = WorkoutType.NotSet;
                            }
                        }
                    }
                    WeekData.Week.day[i].workouts.workout = new Models.Exercise.weeksWeekDayWorkoutsWorkout();

                    WeekData.Week.day[i].workouts.workout.workoutdescription = workout.WorkoutDescription;
                    WeekData.Week.day[i].workouts.workout.description = workout.Description;
                    WeekData.Week.day[i].workouts.workout.WorkoutId = workout.Id;
                    WeekData.Week.day[i].workouts.workout.WeekDayNo = workout.WeekDayNo;
                    WeekData.Week.day[i].workouts.workout.WeekNo = workout.WeekNo;
                    WeekData.Week.day[i].workouts.workout.name = workout.WorkoutName;
                    WeekData.Week.day[i].workouts.workout.WorkoutLevel = workout.WorkoutLevel;
                    WeekData.Week.day[i].workouts.workout.WorkoutType = workout.WorkoutType;
                    if (workout.WarmupTime.HasValue)
                    {
                        WeekData.Week.day[i].workouts.workout.warmup = workout.WarmupTime.Value;
                    }
                    WeekData.Week.day[i].workouts.workout.excercises = new Models.Exercise.weeksWeekDayWorkoutsWorkoutExcercise[workout.Exercises.Count()];
                    if (workout.Exercises != null)
                    {
                        for (int j = 0; j < workout.Exercises.Count(); j++)
                        {
                            WeekData.Week.day[i].workouts.workout.excercises[j] = new Models.Exercise.weeksWeekDayWorkoutsWorkoutExcercise();
                            WeekData.Week.day[i].workouts.workout.excercises[j].description = workout.Exercises[j].Description;
                            WeekData.Week.day[i].workouts.workout.excercises[j].image = workout.Exercises[j].Image;
                            WeekData.Week.day[i].workouts.workout.excercises[j].name = workout.Exercises[j].Name;
                            WeekData.Week.day[i].workouts.workout.excercises[j].SetsRep = workout.Exercises[j].SetsRep;
                            if (workout.Exercises[j].Time.HasValue)
                            {
                                WeekData.Week.day[i].workouts.workout.excercises[j].time = workout.Exercises[j].Time.Value;
                            }
                        }
                    //}
                    i++;
                    startDate = startDate.AddDays(1);
                    }
                });
            }

            
            response = Request.CreateResponse<Models.Exercise.WeekData>(HttpStatusCode.OK, WeekData);
            return response;
        }
        [HttpGet]
        [ActionName("GetExerciseByLevel")]
        [ResponseType(typeof(BootcampDto.ExercisesByLevelResponse))]
        public HttpResponseMessage GetExerciseByLevel(int bootCampId, WorkoutLevel level)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>().FindById(userId);

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(bootCampId);

            int DayNo = ((DateTime.UtcNow.Date - bootCamp.StartDate).Days) + 1;
            int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
            int weekDayNo = DayNo - (7 * (weekNo - 1));

            List<Bootcamp.Workout> workouts = uow.Repository<Bootcamp.Workout>()
                .Query()
                .Include(t => t.Exercises)
                .Filter(o => o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.WorkoutLevel == level)
                .Get()
                .ToList();

            BootCamp.DomainObjects.BootCamp.ActivityLog activityLog = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserId == bootCampUser.Id && o.DayNo == DayNo && o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.LogType == LogType.Workout)
                    .Get().FirstOrDefault();

            BootcampDto.ExercisesByLevelResponse workoutResponse = new BootcampDto.ExercisesByLevelResponse();
            
            if (workouts != null && workouts.Count() > 0)
            {
                workoutResponse.WorkOuts = new List<Models.Exercise.weeksWeekDayWorkouts>(workouts.Count());

                workouts.ForEach(workout =>
                {
                    Models.Exercise.weeksWeekDayWorkouts workoutDto = new Models.Exercise.weeksWeekDayWorkouts();
                    workoutDto.workout = new Models.Exercise.weeksWeekDayWorkoutsWorkout();
                    workoutDto.workout.workoutdescription = workout.WorkoutDescription;
                    workoutDto.ActivityLogId = 0;
                    workoutDto.ActivityStatus = ActivityStatus.Undefined;
                    if (activityLog != null)
                    {
                        workoutDto.ActivityLogId = activityLog.Id;
                        workoutDto.ActivityStatus = activityLog.ActivityStatus;
                    }
                    workoutDto.workout.description = workout.Description;
                    workoutDto.workout.WorkoutId = workout.Id;
                    workoutDto.workout.name = workout.WorkoutName;
                    workoutDto.workout.WorkoutLevel = workout.WorkoutLevel;
                    workoutDto.workout.WorkoutType = workout.WorkoutType;
                    if (workout.WarmupTime.HasValue)
                    {
                        workoutDto.workout.warmup = workout.WarmupTime.Value;
                    }
                    workoutDto.workout.excercises = new Models.Exercise.weeksWeekDayWorkoutsWorkoutExcercise[workout.Exercises.Count()];
                    if (workoutDto.workout.excercises != null)
                    {
                        for (int i = 0; i < workoutDto.workout.excercises.Length; i++)
                        {
                            workoutDto.workout.excercises[i] = new Models.Exercise.weeksWeekDayWorkoutsWorkoutExcercise();
                            workoutDto.workout.excercises[i].description = workout.Exercises[i].Description;
                            workoutDto.workout.excercises[i].image = workout.Exercises[i].Image;
                            workoutDto.workout.excercises[i].name = workout.Exercises[i].Name;
                            workoutDto.workout.excercises[i].SetsRep = workout.Exercises[i].SetsRep;
                            if (workout.Exercises[i].Time.HasValue)
                            {
                                workoutDto.workout.excercises[i].time = workout.Exercises[i].Time.Value;
                            }
                        }
                    }
                    workoutResponse.WorkOuts.Add(workoutDto);
                });
            }
            response = Request.CreateResponse<BootcampDto.ExercisesByLevelResponse>(HttpStatusCode.OK, workoutResponse);
            return response;
        }

        [HttpGet]
        [ActionName("GetExerciseByDayWeek")]
        [ResponseType(typeof(BootcampDto.ExercisesByLevelResponse))]
        public HttpResponseMessage GetExerciseByDayWeek(int bootCampId, WorkoutLevel level,int weekDayNo,int weekNo)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>().FindById(userId);

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(bootCampId);

            //int DayNo = ((DateTime.UtcNow.Date - bootCamp.StartDate).Days) + 1;
            //int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
            //int weekDayNo = DayNo - (7 * (weekNo - 1));

            List<Bootcamp.Workout> workouts = uow.Repository<Bootcamp.Workout>()
                .Query()
                .Include(t => t.Exercises)
                .Filter(o => o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.WorkoutLevel == level)
                .Get()
                .ToList();

            BootCamp.DomainObjects.BootCamp.ActivityLog activityLog = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserId == bootCampUser.Id 
                    && o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.LogType == LogType.Workout)
                    .Get().FirstOrDefault();

            BootcampDto.ExercisesByLevelResponse workoutResponse = new BootcampDto.ExercisesByLevelResponse();
            
            if (workouts != null && workouts.Count() > 0)
            {
                workoutResponse.WorkOuts = new List<Models.Exercise.weeksWeekDayWorkouts>(workouts.Count());

                workouts.ForEach(workout =>
                {
                    if (workout.Exercises != null)
                    {
                        workout.Exercises = workout.Exercises.OrderBy(o => o.Id).ToList();
                    }
                    Models.Exercise.weeksWeekDayWorkouts workoutDto = new Models.Exercise.weeksWeekDayWorkouts();
                    workoutDto.workout = new Models.Exercise.weeksWeekDayWorkoutsWorkout();
                    workoutDto.workout.workoutdescription = workout.WorkoutDescription;
                    workoutDto.ActivityLogId = 0;
                    workoutDto.ActivityStatus = ActivityStatus.Undefined;
                    if (activityLog != null)
                    {
                        workoutDto.ActivityLogId = activityLog.Id;
                        workoutDto.ActivityStatus = activityLog.ActivityStatus;
                    }
                    workoutDto.workout.description = workout.Description;
                    workoutDto.workout.WorkoutId = workout.Id;
                    workoutDto.workout.name = workout.WorkoutName;
                    workoutDto.workout.WorkoutLevel = workout.WorkoutLevel;
                    workoutDto.workout.WorkoutType = workout.WorkoutType;
                    if (workout.WarmupTime.HasValue)
                    {
                        workoutDto.workout.warmup = workout.WarmupTime.Value;
                    }
                    workoutDto.workout.excercises = new Models.Exercise.weeksWeekDayWorkoutsWorkoutExcercise[workout.Exercises.Count()];
                    if (workoutDto.workout.excercises != null)
                    {
                        for (int i = 0; i < workoutDto.workout.excercises.Length; i++)
                        {
                            workoutDto.workout.excercises[i] = new Models.Exercise.weeksWeekDayWorkoutsWorkoutExcercise();
                            workoutDto.workout.excercises[i].description = workout.Exercises[i].Description;
                            workoutDto.workout.excercises[i].image = workout.Exercises[i].Image;
                            workoutDto.workout.excercises[i].name = workout.Exercises[i].Name;
                            workoutDto.workout.excercises[i].SetsRep = workout.Exercises[i].SetsRep;
                            if (workout.Exercises[i].Time.HasValue)
                            {
                                workoutDto.workout.excercises[i].time = workout.Exercises[i].Time.Value;
                            }
                        }
                    }
                    workoutResponse.WorkOuts.Add(workoutDto);
                });
            }
            response = Request.CreateResponse<BootcampDto.ExercisesByLevelResponse>(HttpStatusCode.OK, workoutResponse);
            return response;
        }

        [HttpGet]
        [ActionName("GetExerciseByTypeLevel")]
        [ResponseType(typeof(Models.Exercise.weeksWeekDayWorkouts))]
        public HttpResponseMessage GetExerciseByTypeLevel(int bootCampId, WorkoutType workoutType, WorkoutLevel level)
        {
            HttpResponseMessage response = null;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUserDetail bootCampUser = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Filter(o=>o.UserId == userId && o.BootCampId == bootCampId)
                .Get()
                .FirstOrDefault();
            
            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(bootCampId);

            int DayNo = ((DateTime.UtcNow.Date - bootCamp.StartDate).Days) + 1;
            int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
            int weekDayNo = DayNo - (7 * (weekNo - 1));

            Bootcamp.Workout workout = uow.Repository<Bootcamp.Workout>()
                .Query()
                .Include(t=>t.Exercises)
                .Filter(o => o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.WorkoutLevel == level && o.WorkoutType == workoutType)
                .Get()
                .FirstOrDefault();

            BootCamp.DomainObjects.BootCamp.ActivityLog activityLog = uow.Repository<BootCamp.DomainObjects.BootCamp.ActivityLog>().Query()
                    .Filter(o => o.BootCampUserId == bootCampUser.Id && o.DayNo == DayNo && o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.LogType == LogType.Workout)
                    .Get().FirstOrDefault();

            Models.Exercise.weeksWeekDayWorkouts workoutResponse = new Models.Exercise.weeksWeekDayWorkouts();
            if(activityLog != null)
            {
                workoutResponse.ActivityLogId = activityLog.Id;
                workoutResponse.ActivityStatus = activityLog.ActivityStatus;
            }
            if (workout != null)
            {
                workoutResponse.workout = new Models.Exercise.weeksWeekDayWorkoutsWorkout();
                workoutResponse.workout.workoutdescription = workout.WorkoutDescription;
                workoutResponse.workout.WorkoutId = workout.Id;
                workoutResponse.workout.description = workout.Description;
                workoutResponse.workout.name = workout.WorkoutName;
                workoutResponse.workout.WorkoutLevel = workout.WorkoutLevel;
                workoutResponse.workout.WorkoutType = workout.WorkoutType;
                if (workout.WarmupTime.HasValue)
                {
                    workoutResponse.workout.warmup = workout.WarmupTime.Value;
                }
                workoutResponse.workout.excercises = new Models.Exercise.weeksWeekDayWorkoutsWorkoutExcercise[workout.Exercises.Count()];
                workout.Exercises = workout.Exercises.OrderBy(o => o.Id).ToList();
                if (workoutResponse.workout.excercises != null)
                {
                    for(int i=0;i< workoutResponse.workout.excercises.Length;i++)
                    {
                        workoutResponse.workout.excercises[i] = new Models.Exercise.weeksWeekDayWorkoutsWorkoutExcercise();
                        workoutResponse.workout.excercises[i].description = workout.Exercises[i].Description;
                        workoutResponse.workout.excercises[i].image = workout.Exercises[i].Image;
                        workoutResponse.workout.excercises[i].name = workout.Exercises[i].Name;
                        workoutResponse.workout.excercises[i].SetsRep = workout.Exercises[i].SetsRep;
                        if (workout.Exercises[i].Time.HasValue)
                        {
                            workoutResponse.workout.excercises[i].time = workout.Exercises[i].Time.Value;
                        }
                    }
                }
            }
            response = Request.CreateResponse<Models.Exercise.weeksWeekDayWorkouts>(HttpStatusCode.OK, workoutResponse);
            return response;
        }
        [HttpPost]
        [ActionName("LeaveBootCamp")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        [Authorize]
        [ValidateModel]
        public HttpResponseMessage LeaveBootCamp(BootcampDto.LeaveBootCampRequest request)
        {
            HttpResponseMessage response = null;
            try
            {
                ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

                Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
                int userId = int.Parse(claim.Value);

                Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                    .Query()
                    .Include(c => c.BootCamp)
                    .Filter(c => c.UserId == userId && c.BootCampId == request.BootCampId)
                    .Get()
                    .FirstOrDefault();
                if (bootCampUserDetail != null)
                {
                    bootCampUserDetail.DroppedOut = true;
                    bootCampUserDetail.UpdatedOn = DateTime.Now;
                    bootCampUserDetail.State = ObjectInterfaces.ObjectState.Modified;
                    bootCampUserDetail.BootCamp.DroppedOff = bootCampUserDetail.BootCamp.DroppedOff + 1;
                    bootCampUserDetail.BootCamp.SignedUp = bootCampUserDetail.BootCamp.SignedUp - 1;
                    bootCampUserDetail.BootCamp.UpdatedOn = DateTime.Now;
                    bootCampUserDetail.BootCamp.State = ObjectInterfaces.ObjectState.Modified;
                    uow.Repository<Bootcamp.BootCampUserDetail>().Update(bootCampUserDetail);
                    uow.Save();

                    List<UserNotification> userNotifications = notificationUow.Repository<UserNotification>()
                            .Query()
                            .Filter(o => o.ForUserId == userId && (o.NotificationForId.HasValue == true && o.NotificationForId.Value == request.BootCampId))
                            .Get()
                            .ToList();

                    userNotifications.ForEach((userNotification) =>
                    {
                        userNotification.State = ObjectInterfaces.ObjectState.Deleted;
                        notificationUow.Repository<UserNotification>().Delete(userNotification);
                    });
                    notificationUow.Save();

                    response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse("Removed from bootcamp."));
                }
                else
                {
                    response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.NotFound, new BootcampDto.ErrorResponse("User not found. "+ userId + "bootcamp: " +request.BootCampId));
                }
                
            }
            catch(Exception ex)
            {
                response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.NotFound, new BootcampDto.ErrorResponse(ex.Message));
            }
            return response;
        }
        [HttpPost]
        [ActionName("LogActivity")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        [Authorize]
        public HttpResponseMessage LogActivity(BootcampDto.LogActivityRequest request)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            int BootCampUserId = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Filter(o => o.BootCampId == request.BootCampId && o.UserId == userId)
                .Get()
                .Select(t => t.Id)
                .FirstOrDefault();

            Bootcamp.ActivityLog activityLog = uow.Repository<Bootcamp.ActivityLog>().FindById(request.Id);

            if(activityLog != null)
            {
                activityLog.Image = request.Image;
                activityLog.Description = request.Description;
                activityLog.LogPrivacyStatus = request.LogStatus;
                activityLog.ActivityStatus = request.ActivityStatus;
                activityLog.LogType = request.LogType;
                activityLog.LogTime = DateTime.UtcNow;
                activityLog.Duration = request.Duration;
                if(request.LogType == LogType.Workout)
                {
                    activityLog.WorkoutId = request.WorkoutId;
                    activityLog.WorkoutLevel = request.WorkoutLevel;
                }
                activityLog.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<Bootcamp.ActivityLog>().Update(activityLog);
                uow.Save();
            }

            HttpResponseMessage response = Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse("Activity logged."));
            return response;
        }

        [HttpGet]
        [ActionName("MissedMealSummary")]
        [ResponseType(typeof(BootcampDto.MissedMealResponse))]
        [Authorize]
        public HttpResponseMessage MissedMealSummary(int logId)
        {
            BootcampDto.MissedMealResponse response = new BootcampDto.MissedMealResponse();

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>().FindById(userId);

            Bootcamp.ActivityLog activityLog = uow.Repository<Bootcamp.ActivityLog>().FindById(logId);

            List<Bootcamp.Meal> meals = uow.Repository<Bootcamp.Meal>().Query()
                    .Include(o=>o.Recipe)
                    .Filter(o => 
                    o.MealType == activityLog.MealType.Value 
                    && o.DayOfWeek == activityLog.WeekDayNo 
                    && o.WeekNumber == activityLog.WeekNo 
                    && o.DietTypeId == bootCampUser.MealPlan)
                    .Get()
                    .ToList();

            response.MealSummary = new BootcampDto.MissedMealSummary();
            response.MealSummary.MealType = activityLog.MealType.Value;
            response.MealSummary.MealTaken = activityLog.ActivityStatus;
            if (activityLog.ActivityStatus != ActivityStatus.Undefined)
            {
                response.MealSummary.Id = activityLog.Id;
            }
            //response.MealSummary.MealTime = activityLog.LogTime.AddMinutes(bootCampUser.TimeZoneOffset);
            response.MealSummary.Image = activityLog.Image;
            response.MealSummary.Recipies = new List<BootcampDto.MissedRecipieDetailDto>();
            meals.ForEach(meal =>
            {
                BootcampDto.MissedRecipieDetailDto recipeDto = new BootcampDto.MissedRecipieDetailDto();
                recipeDto.Name = meal.Recipe.Name;
                response.MealSummary.Recipies.Add(recipeDto);
            });
            return Request.CreateResponse<BootcampDto.MissedMealResponse>(HttpStatusCode.OK, response);
        }
        [HttpGet]
        [ActionName("UserDailySummary")]
        [ResponseType(typeof(BootcampDto.UserBootCampDaySummaryResponse))]
        [Authorize]
        public HttpResponseMessage UserDailySummary(int BootCampId)
        {
            BootcampDto.UserBootCampDaySummaryResponse response = new BootcampDto.UserBootCampDaySummaryResponse();
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>()
                .Query()
                .Include(o => o.UserNotificationSetting)
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            Bootcamp.BootCampUserDetail bootCampUserdetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Filter(o => o.UserId == userId && o.BootCampId == BootCampId)
                .Get()
                .FirstOrDefault();

            DateTime userDateTime = DateTime.UtcNow.AddMinutes(bootCampUser.TimeZoneOffset);

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(BootCampId);

            int DayNo = ((userDateTime.Date - bootCamp.StartDate).Days) + 1;
            int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
            int weekDayNo = DayNo - (7 * (weekNo - 1));

            response.SummaryDay = userDateTime;

            List<Bootcamp.ActivityLog> activityLogs = uow.Repository<Bootcamp.ActivityLog>().Query()
                .Filter(o => o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.DayNo == DayNo && o.BootCampUserId == bootCampUserdetail.Id)
                .Get().ToList();
            response.MealSummary = new List<BootcampDto.MealSummary>();
            activityLogs.ForEach(activityLog =>
            {
                if (activityLog.LogType == LogType.Workout)
                {
                    Bootcamp.Workout workout = uow.Repository<Bootcamp.Workout>().Query().Include(t => t.Exercises)
                    .Filter(o => o.Id == activityLog.WorkoutId).Get().FirstOrDefault();
                    response.WorkoutSummary = new BootcampDto.WorkoutSummary();
                    response.WorkoutSummary.Image = activityLog.Image;
                    if (activityLog.Duration.HasValue)
                    {
                        response.WorkoutSummary.CompletionTime = activityLog.Duration.Value;
                    }
                    if (activityLog.ActivityStatus != ActivityStatus.Undefined)
                    {
                        response.WorkoutSummary.Id = activityLog.Id;
                    }
                    response.WorkoutSummary.WorkoutName = workout.WorkoutName;
                    response.WorkoutSummary.ExerciseTime = activityLog.LogTime.AddMinutes(bootCampUser.TimeZoneOffset);
                    response.WorkoutSummary.Excercises = new List<BootcampDto.ExcerciseDetailDto>();
                    workout.Exercises.ToList().ForEach(exercise =>
                    {
                        BootcampDto.ExcerciseDetailDto excerciseDto = new BootcampDto.ExcerciseDetailDto();
                        excerciseDto.Name = exercise.Name;
                        response.WorkoutSummary.Excercises.Add(excerciseDto);
                    });
                }
                else if (activityLog.LogType == LogType.Meal)
                {
                    List<Bootcamp.Meal> meals = uow.Repository<Bootcamp.Meal>().Query()
                    .Include(t => t.Recipe.RecipeIngredients)
                    .Include(t => t.Recipe.RecipeInstructions)
                    .Filter(o => o.MealType == activityLog.MealType.Value && o.DayOfWeek == activityLog.WeekDayNo && o.WeekNumber == activityLog.WeekNo && o.DietTypeId == bootCampUser.MealPlan).Get().ToList();

                    BootcampDto.MealSummary mealSummary = new BootcampDto.MealSummary();
                    mealSummary.MealType = activityLog.MealType.Value;
                    mealSummary.MealTaken = activityLog.ActivityStatus;
                    if (activityLog.ActivityStatus != ActivityStatus.Undefined)
                    {
                        mealSummary.Id = activityLog.Id;
                    }
                    mealSummary.MealTime = activityLog.LogTime.AddMinutes(bootCampUser.TimeZoneOffset);
                    mealSummary.Image = activityLog.Image;
                    mealSummary.Recipies = new List<weeksWeekDayMealRecipe>();
                    meals.ForEach(meal =>
                    {
                        weeksWeekDayMealRecipe recipeDto = new weeksWeekDayMealRecipe();
                        if (meal.Recipe.Cooktime.HasValue)
                        {
                            recipeDto.cooktime = meal.Recipe.Cooktime.Value;
                        }
                        recipeDto.Id = meal.Recipe.Id;
                        int favCount = uow.Repository<Bootcamp.FavoriteRecipe>().Query()
                            .Filter(o => o.UserId == userId && o.RecipeId == meal.Recipe.Id && o.Favorite == MarkAs.Favorite).Get().Count();
                        recipeDto.IsFavorite = favCount > 0 ? true : false;
                        recipeDto.image = meal.Recipe.Image;
                        recipeDto.name = meal.Recipe.Name;
                        if (meal.Recipe.Readintime.HasValue)
                        {
                            recipeDto.readintime = meal.Recipe.Readintime.Value;
                        }
                        recipeDto.sequenceSpecified = false;
                        if (meal.Recipe.Preptime.HasValue)
                        {
                            recipeDto.sequence = meal.Recipe.Preptime.Value;
                        }
                        if (meal.Recipe.RecipeInstructions != null && meal.Recipe.RecipeInstructions.Count() > 0)
                        {
                            recipeDto.directions = new weeksWeekDayMealRecipeDirection[meal.Recipe.RecipeInstructions.Count()];
                            int count = 0;
                            meal.Recipe.RecipeInstructions.ToList().ForEach(instruction =>
                            {
                                recipeDto.directions[count] = new weeksWeekDayMealRecipeDirection();
                                recipeDto.directions[count].sequence = instruction.Sequence;
                                recipeDto.directions[count].Value = instruction.Instruction;
                                count++;
                            });
                        }
                        if (meal.Recipe.RecipeIngredients != null && meal.Recipe.RecipeIngredients.Count() > 0)
                        {
                            recipeDto.ingredients = new weeksWeekDayMealRecipeIngredient[meal.Recipe.RecipeIngredients.Count()];
                            int count = 0;
                            meal.Recipe.RecipeIngredients.ToList().ForEach(ingredient =>
                            {
                                recipeDto.ingredients[count] = new weeksWeekDayMealRecipeIngredient();
                                recipeDto.ingredients[count].name = ingredient.Ingredient;
                                count++;
                            });
                        }

                        mealSummary.Recipies.Add(recipeDto);
                    });
                    response.MealSummary.Add(mealSummary);
                }
            });
            return Request.CreateResponse<BootcampDto.UserBootCampDaySummaryResponse>(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [ActionName("UserDailySummaryByLogId")]
        [ResponseType(typeof(BootcampDto.UserBootCampDaySummaryResponse))]
        [Authorize]
        public HttpResponseMessage UserDailySummaryByLogId(int logId)
        {
            BootcampDto.UserBootCampDaySummaryResponse response = new BootcampDto.UserBootCampDaySummaryResponse();
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>()
                .Query()
                .Include(o => o.UserNotificationSetting)
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            //Bootcamp.BootCampUserDetail bootCampUserdetail = uow.Repository<Bootcamp.BootCampUserDetail>()
            //    .Query()
            //    .Filter(o => o.UserId == userId && o.BootCampId == BootCampId)
            //    .Get()
            //    .FirstOrDefault();

            DateTime userDateTime = DateTime.UtcNow.AddMinutes(bootCampUser.TimeZoneOffset);

            //Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(BootCampId);

            //int DayNo = ((userDateTime.Date - bootCamp.StartDate).Days) + 1;
            //int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
            //int weekDayNo = DayNo - (7 * (weekNo - 1));

            response.SummaryDay = userDateTime;

            List<Bootcamp.ActivityLog> activityLogs = uow.Repository<Bootcamp.ActivityLog>().Query()
                .Filter(o => o.Id == logId)
                .Get().ToList();
            response.MealSummary = new List<BootcampDto.MealSummary>();
            activityLogs.ForEach(activityLog =>
            {
                if(activityLog.LogType == LogType.Workout)
                {
                    Bootcamp.Workout workout = uow.Repository<Bootcamp.Workout>().Query().Include(t=>t.Exercises)
                    .Filter(o=>o.Id == activityLog.WorkoutId).Get().FirstOrDefault();
                    response.WorkoutSummary = new BootcampDto.WorkoutSummary();
                    response.WorkoutSummary.Image = activityLog.Image;
                    if (activityLog.Duration.HasValue)
                    {
                        response.WorkoutSummary.CompletionTime = activityLog.Duration.Value;
                    }
                    if (activityLog.ActivityStatus != ActivityStatus.Undefined)
                    {
                        response.WorkoutSummary.Id = activityLog.Id;
                    }
                    response.WorkoutSummary.WorkoutName = workout.WorkoutName;
                    response.WorkoutSummary.ExerciseTime = activityLog.LogTime.AddMinutes(bootCampUser.TimeZoneOffset);
                    response.WorkoutSummary.Excercises = new List<BootcampDto.ExcerciseDetailDto>();
                    if(workout.Exercises != null)
                    {
                        workout.Exercises = workout.Exercises.OrderBy(o => o.Id).ToList();
                    }
                    workout.Exercises.ToList().ForEach(exercise =>
                    {
                        BootcampDto.ExcerciseDetailDto excerciseDto = new BootcampDto.ExcerciseDetailDto();
                        excerciseDto.Name = exercise.Name;
                        excerciseDto.SetsReps = exercise.SetsRep;
                        if(exercise.Time.HasValue)
                        {
                            excerciseDto.Time = exercise.Time.Value;
                        }
                        response.WorkoutSummary.Excercises.Add(excerciseDto);
                    });
                }
                else if(activityLog.LogType == LogType.Meal)
                {
                    List<Bootcamp.Meal> meals = uow.Repository<Bootcamp.Meal>().Query()
                    .Include(t => t.Recipe.RecipeIngredients)
                    .Include(t => t.Recipe.RecipeInstructions)
                    .Filter(o => o.MealType == activityLog.MealType.Value && o.DayOfWeek == activityLog.WeekDayNo && o.WeekNumber == activityLog.WeekNo && o.DietTypeId == bootCampUser.MealPlan).Get().ToList();

                    BootcampDto.MealSummary mealSummary = new BootcampDto.MealSummary();
                    mealSummary.MealType = activityLog.MealType.Value;
                    mealSummary.MealTaken = activityLog.ActivityStatus;
                    if(activityLog.ActivityStatus != ActivityStatus.Undefined)
                    {
                        mealSummary.Id = activityLog.Id;
                    }
                    mealSummary.MealTime = activityLog.LogTime.AddMinutes(bootCampUser.TimeZoneOffset);
                    mealSummary.Image = activityLog.Image;
                    mealSummary.Recipies = new List<weeksWeekDayMealRecipe>();
                    meals.ForEach(meal =>
                    {
                        weeksWeekDayMealRecipe recipeDto = new weeksWeekDayMealRecipe();
                        if (meal.Recipe.Cooktime.HasValue)
                        {
                            recipeDto.cooktime = meal.Recipe.Cooktime.Value;
                        }
                        recipeDto.Id = meal.Recipe.Id;
                        int favCount = uow.Repository<Bootcamp.FavoriteRecipe>().Query()
                            .Filter(o => o.UserId == userId && o.RecipeId == meal.Recipe.Id && o.Favorite == MarkAs.Favorite).Get().Count();
                        recipeDto.IsFavorite = favCount > 0? true:false;
                        recipeDto.image = meal.Recipe.Image;
                        recipeDto.name = meal.Recipe.Name;
                        if (meal.Recipe.Readintime.HasValue)
                        {
                            recipeDto.readintime = meal.Recipe.Readintime.Value;
                        }
                        recipeDto.sequenceSpecified = false;
                        if (meal.Recipe.Preptime.HasValue)
                        {
                            recipeDto.sequence = meal.Recipe.Preptime.Value;
                        }
                        if (meal.Recipe.RecipeInstructions != null && meal.Recipe.RecipeInstructions.Count() > 0)
                        {
                            recipeDto.directions = new weeksWeekDayMealRecipeDirection[meal.Recipe.RecipeInstructions.Count()];
                            int count = 0;
                            meal.Recipe.RecipeInstructions.ToList().ForEach(instruction =>
                            {
                                recipeDto.directions[count] = new weeksWeekDayMealRecipeDirection();
                                recipeDto.directions[count].sequence = instruction.Sequence;
                                recipeDto.directions[count].Value = instruction.Instruction;
                                count++;
                            });
                        }
                        if (meal.Recipe.RecipeIngredients != null && meal.Recipe.RecipeIngredients.Count() > 0)
                        {
                            recipeDto.ingredients = new weeksWeekDayMealRecipeIngredient[meal.Recipe.RecipeIngredients.Count()];
                            int count = 0;
                            meal.Recipe.RecipeIngredients.ToList().ForEach(ingredient =>
                            {
                                recipeDto.ingredients[count] = new weeksWeekDayMealRecipeIngredient();
                                recipeDto.ingredients[count].name = ingredient.Ingredient;
                                count++;
                            });
                        }

                        mealSummary.Recipies.Add(recipeDto);
                    });
                    response.MealSummary.Add(mealSummary);
                }
            });
            return Request.CreateResponse<BootcampDto.UserBootCampDaySummaryResponse>(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [ActionName("TodayWorkoutStatus")]
        [ResponseType(typeof(BootcampDto.WorkoutStatusResponse))]
        [Authorize]
        public HttpResponseMessage TodayWorkoutStatus(int BootCampId)
        {
            BootcampDto.WorkoutStatusResponse response = new BootcampDto.WorkoutStatusResponse();
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>()
                .Query()
                .Include(o => o.UserNotificationSetting)
                .Filter(o => o.Id == userId)
                .Get()
                .FirstOrDefault();

            Bootcamp.BootCampUserDetail bootCampUserdetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Filter(o => o.UserId == userId && o.BootCampId == BootCampId)
                .Get()
                .FirstOrDefault();

            DateTime userDateTime = DateTime.UtcNow.AddMinutes(bootCampUser.TimeZoneOffset);

            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().FindById(BootCampId);

            int DayNo = ((userDateTime.Date - bootCamp.StartDate).Days) + 1;
            int weekNo = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DayNo) / 7.0));
            int weekDayNo = DayNo - (7 * (weekNo - 1));

            Bootcamp.ActivityLog activityLog = uow.Repository<Bootcamp.ActivityLog>().Query()
                .Filter(o => o.WeekDayNo == weekDayNo && o.WeekNo == weekNo && o.DayNo == DayNo && o.BootCampUserId == bootCampUserdetail.Id && o.LogType==LogType.Workout)
                .Get().FirstOrDefault();

            if(activityLog != null)
            {
                response.WorkoutActivityStatus = activityLog.ActivityStatus;
                if (activityLog.WorkoutId.HasValue)
                {
                    Bootcamp.Workout workout = uow.Repository<Bootcamp.Workout>().FindById(activityLog.WorkoutId.Value);
                    response.WorkoutType = workout.WorkoutType;
                }
                else
                {
                    response.WorkoutType = WorkoutType.NotSet;
                }
                //response.LogTime = activityLog.LogTime.AddMinutes(bootCampUser.TimeZoneOffset);
            }
            else
            {
                response.WorkoutActivityStatus = ActivityStatus.Undefined;
            }
            
            return Request.CreateResponse<BootcampDto.WorkoutStatusResponse>(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [ActionName("WorkoutSummary")]
        [ResponseType(typeof(BootcampDto.WorkoutSummaryResponse))]
        [Authorize]
        public HttpResponseMessage WorkoutSummary(int BootCampId)
        {
            HttpResponseMessage response = Request.CreateResponse<BootcampDto.WorkoutSummaryResponse>(HttpStatusCode.OK, new BootcampDto.WorkoutSummaryResponse());
            return response;
        }

        [HttpPost]
        [ActionName("InviteBootCamp")]
        [ResponseType(typeof(BootcampDto.SuccessResponse))]
        [Authorize]
        public HttpResponseMessage InviteBootCamp(BootcampDto.BootCampInvite request)
        {
            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>()
                .Query()
                .Include(p=>p.BootCampInvitedUsers)
                .Include(p => p.UserNotifications)
                .Filter(o=> o.Id == request.BootCampId)
                .Get()
                .FirstOrDefault();
            Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>().FindById(bootCamp.CreatorId);
            if (bootCamp.BootCampInvitedUsers == null)
            {
                bootCamp.BootCampInvitedUsers = new List<Bootcamp.BootCampInvitedUsers>();
            }
            if (bootCamp.UserNotifications == null)
            {
                bootCamp.UserNotifications = new List<UserNotification>();
            }
            request.InvitedUser.ForEach((user) =>
            {
                if (bootCamp.BootCampInvitedUsers.Where(o => o.UserId == user.Id).Count() == 0)
                {


                    Bootcamp.BootCampInvitedUsers invitedUser = new Bootcamp.BootCampInvitedUsers();
                    UserNotification userNotification = new UserNotification();

                    userNotification.NotificationType = NotificationType.JoinBootCamp;
                    userNotification.ForUserId = user.Id;
                    userNotification.SentByUserId = bootCamp.CreatorId;
                    userNotification.IsDeleted = false;
                    userNotification.BootCamp = bootCamp;
                    userNotification.CreatedOn = DateTime.UtcNow;
                    userNotification.UpdatedOn = DateTime.UtcNow;
                    userNotification.Title = bootCampUser.FullName;
                    userNotification.TextLine1 = "You have been invited to the Boot Camp: ";
                    userNotification.TextLine2 = bootCamp.Name;
                    userNotification.State = ObjectInterfaces.ObjectState.Added;
                    bootCamp.UserNotifications.Add(userNotification);

                    invitedUser.UserId = user.Id;
                    invitedUser.BootCamp = bootCamp;
                    invitedUser.State = ObjectInterfaces.ObjectState.Added;
                    bootCamp.State = ObjectInterfaces.ObjectState.Modified;
                    bootCamp.BootCampInvitedUsers.Add(invitedUser);
                }
            });
            uow.Repository<Bootcamp.BootCamp>().Update(bootCamp);
            uow.Save();

            return Request.CreateResponse<BootcampDto.SuccessResponse>(HttpStatusCode.OK, new BootcampDto.SuccessResponse());
        }

        [HttpPost]
        [ActionName("RestartBootCamp")]
        [ResponseType(typeof(BootcampDto.BootCampCreateResponse))]
        [Authorize]
        public HttpResponseMessage RestartBootCamp(BootcampDto.RestartBootCampRequest request)
        {
            HttpResponseMessage response = null;

            if (!ModelState.IsValid)
            {
                return response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest,
                    new BootcampDto.ErrorResponse(ErrorHelpers.ModelStateErrors(ModelState)));
            }

            if (request.StartDate.Date <= DateTime.UtcNow.Date)
            {
                return response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest,
                    new BootcampDto.ErrorResponse(Resources.StringResources.BootCampStartDayError));
            }

            DateTime timeStamp = DateTime.UtcNow;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value); 

            claim = currentClaimsPrincipal.FindFirst(ClaimTypes.Email);
            string email = claim.Value;  
            try
            {
                Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>().FindById(userId);
                if (bootCampUser.AmountPaid.HasValue)
                {
                    int count = uow.Repository<Bootcamp.BootCamp>()
                    .Query()
                    .Filter(o => (o.CreatorId == bootCampUser.Id) &&
                    (request.StartDate >= o.StartDate && request.StartDate <= o.EndDate && o.BootCampStatus == BootCampStatus.Active))
                    .Get().Count();

                    if (count > 0)
                    {
                        return Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest, new BootcampDto.ErrorResponse(Resources.StringResources.BootcampError1));
                    }
                    else
                    {
                        Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>().Query()
                            .Include(p => p.BootCampUserDetails.Select(t=>t.BootCampUser))
                            .Filter(o => o.Id == request.BootCampId).Get().FirstOrDefault();
                        Bootcamp.BootCamp bootCampCopy = new Bootcamp.BootCamp
                        {
                            Name = bootCamp.Name,
                            About = bootCamp.About,
                            BootcampImage = bootCamp.BootcampImage,
                            CreatedOn = timeStamp,
                            UpdatedOn = timeStamp,
                            StartDate = request.StartDate.Date,
                            EndDate = request.StartDate.Date.AddDays(6 * 7),
                            DroppedOff = 0,
                            MaxMembers = MAXMEMBERS,
                            SignedUp = 1,
                            IsPrivate = bootCamp.IsPrivate,
                            CreatorId = bootCampUser.Id,
                            Owner = bootCampUser,
                            State = ObjectInterfaces.ObjectState.Added
                        };
                        Bootcamp.BootCampUserDetail BootCampUserDetail = new Bootcamp.BootCampUserDetail();
                        BootCampUserDetail.BootCamp = bootCampCopy;
                        BootCampUserDetail.CreatedOn = DateTime.UtcNow;
                        BootCampUserDetail.UpdatedOn = DateTime.UtcNow;
                        BootCampUserDetail.UserId = bootCampUser.Id;
                        BootCampUserDetail.JoinDate = DateTime.UtcNow;
                        BootCampUserDetail.DroppedOut = false;
                        BootCampUserDetail.State = ObjectInterfaces.ObjectState.Added;
                        if (bootCampCopy.BootCampUserDetails == null)
                        {
                            bootCampCopy.BootCampUserDetails = new List<Bootcamp.BootCampUserDetail>();
                        }
                        bootCampCopy.BootCampUserDetails.Add(BootCampUserDetail);
                        if (bootCamp.BootCampUserDetails != null && bootCamp.BootCampUserDetails.Count > 0)
                        {
                            bootCampCopy.BootCampInvitedUsers = new List<Bootcamp.BootCampInvitedUsers>();
                            bootCampCopy.UserNotifications = new List<UserNotification>();

                            //bootCamp.BootCampUserDetails = bootCamp.BootCampUserDetails.Where(o => o.DroppedOut == false).ToList();

                            bootCamp.BootCampUserDetails.Where(o=>o.DroppedOut == false && o.UserId != userId).ToList().ForEach((user) =>
                            {

                                Bootcamp.BootCampUserDetail userDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                                .Query()
                                .Include(o => o.BootCamp)
                                .Filter(o => o.UserId == user.UserId && o.DroppedOut == false &&
                                (o.BootCamp.BootCampStatus != BootCampStatus.Banned && o.BootCamp.BootCampStatus != BootCampStatus.Deleted))
                                .Get()
                                .OrderByDescending(o=>o.BootCamp.Id).FirstOrDefault();

                                if (userDetail == null || userDetail.BootCamp.EndDate.Date < DateTime.UtcNow)
                                {
                                    Bootcamp.BootCampInvitedUsers invitedUser = new Bootcamp.BootCampInvitedUsers();
                                    UserNotification userNotification = new UserNotification();

                                    userNotification.NotificationType = NotificationType.JoinBootCamp;
                                    userNotification.ForUserId = user.UserId;
                                    userNotification.SentByUserId = userId;
                                    userNotification.IsDeleted = false;
                                    userNotification.BootCamp = bootCampCopy;
                                    userNotification.CreatedOn = timeStamp;
                                    userNotification.UpdatedOn = timeStamp;
                                    userNotification.Title = bootCampUser.FullName;
                                    userNotification.TextLine1 = "You have been invited to the Boot Camp: ";
                                    userNotification.TextLine2 = user.BootCampUser.FullName;
                                    userNotification.State = ObjectInterfaces.ObjectState.Added;
                                    bootCampCopy.UserNotifications.Add(userNotification);

                                    invitedUser.UserId = user.UserId;
                                    invitedUser.BootCamp = bootCampCopy;
                                    invitedUser.State = ObjectInterfaces.ObjectState.Added;
                                    bootCampCopy.BootCampInvitedUsers.Add(invitedUser);
                                }
                            });
                        }
                        try
                        {
                            uow.Repository<Bootcamp.BootCamp>().InsertGraph(bootCampCopy);
                            uow.Save();
                        }
                        catch(Exception ex)
                        {
                            BootcampDto.ErrorResponse errorResponse = new BootcampDto.ErrorResponse();
                            errorResponse.AddError(ex.Message);
                            return Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.InternalServerError, errorResponse);
                        }
                        return Request.CreateResponse<BootcampDto.BootCampCreateResponse>(HttpStatusCode.OK, 
                            new BootcampDto.BootCampCreateResponse
                            {
                                Id = bootCampCopy.Id,
                                CampImage = bootCampCopy.BootcampImage,
                                CampName = bootCampCopy.Name,
                                EndDate = bootCampCopy.EndDate,
                                IsOwner = true,
                                StartDate = bootCampCopy.StartDate,
                                Started = false,
                                Message = Resources.StringResources.BootCampCreated
                            });
                    }
                }
                else
                {
                    return Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.MethodNotAllowed, new BootcampDto.ErrorResponse("Only paid members can start a bootcamp."));
                }
            }
            catch (Exception ex)
            {
                logger.Log<Exception>(LogLevel.Error, ex);
                return response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.InternalServerError,
                    new BootcampDto.ErrorResponse(Resources.StringResources.Exception));
            }
        }
        [HttpGet]
        [ActionName("CompletedBootCampSummary")]
        [ResponseType(typeof(BootcampDto.CompletedBootcampResponse))]
        [Authorize]
        public HttpResponseMessage CompletedBootCampSummary(int BootCampId)
        {
            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;
            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            BootcampDto.CompletedBootcampResponse response = new BootcampDto.CompletedBootcampResponse();
            Bootcamp.BootCamp bootCamp = uow.Repository<Bootcamp.BootCamp>()
                .Query()
                .Filter(o => o.Id == BootCampId)
                .Get()
                .FirstOrDefault();

            response.BootCampId = bootCamp.Id;
            response.BootcampImage = bootCamp.BootcampImage;
            response.BootcampName = bootCamp.Name;
            response.CompletionDate = bootCamp.EndDate;
            response.StartDate = bootCamp.StartDate;
            response.WorkoutRecap = new BootcampDto.WorkoutRecap();
            Bootcamp.BootCampUserDetail bootCampUserDetail = uow.Repository<Bootcamp.BootCampUserDetail>()
                .Query()
                .Include(o => o.ActivityLogs)
                .Filter(o => o.BootCampId == BootCampId && o.UserId == userId)
                .Get()
                .FirstOrDefault();

            bootCampUserDetail.ActivityLogs = bootCampUserDetail.ActivityLogs.OrderByDescending(o => o.LogTime).ToList();

            response.WorkoutRecap.BeginnerWorkout = bootCampUserDetail.ActivityLogs.Where(o => o.WorkoutId.HasValue && o.WorkoutLevel == WorkoutLevel.Beginner && o.ActivityStatus== ActivityStatus.Taken).Count();
            response.WorkoutRecap.IntermediateWorkout = bootCampUserDetail.ActivityLogs.Where(o => o.WorkoutId.HasValue && o.WorkoutLevel == WorkoutLevel.Intermediate && o.ActivityStatus == ActivityStatus.Taken).Count();
            response.WorkoutRecap.ExpertWorkout = bootCampUserDetail.ActivityLogs.Where(o => o.WorkoutId.HasValue && o.WorkoutLevel == WorkoutLevel.Expert && o.ActivityStatus == ActivityStatus.Taken).Count();

            List<int> workOutIds = bootCampUserDetail.ActivityLogs.Where(o => o.WorkoutId.HasValue && o.ActivityStatus == ActivityStatus.Taken).Select(s=>s.WorkoutId.Value).ToList();

            response.WorkoutRecap.GymWorkout = uow.Repository<Bootcamp.Workout>()
                .Query()
                .Filter(o => workOutIds.Contains(o.Id) && o.WorkoutType == WorkoutType.Gym)
                .Get()
                .Count();

            response.WorkoutRecap.HomeWorkout = uow.Repository<Bootcamp.Workout>()
                .Query()
                .Filter(o => workOutIds.Contains(o.Id) && o.WorkoutType == WorkoutType.Home)
                .Get()
                .Count();

            response.BodyPictures = new List<BootcampDto.BodyPictures>();
            bootCampUserDetail.ActivityLogs.ToList().ForEach(log =>
            {
                if (log.WorkoutId.HasValue && !string.IsNullOrEmpty(log.Image))
                {
                    response.BodyPictures.Add(new BootcampDto.BodyPictures { ImageDate = log.LogTime , Images = log.Image });
                }
            });

            List<Measurement> measurements = userUow.Repository<Measurement>()
                .Query()
                .Filter(o => o.UserId == userId && (o.CreatedOn >= bootCamp.StartDate && o.CreatedOn <= bootCamp.EndDate))
                .Get()
                .OrderByDescending(o=>o.CreatedOn)
                .ToList();

            response.BicepsSummary = new List<BootcampDto.BicepsHistory>();
            response.WaistSummary = new List<BootcampDto.WaistHistory>();
            response.WeightSummary = new List<BootcampDto.WeightlossHistory>();
            response.HipsSummary = new List<BootcampDto.HipsHistory>();
            response.ChestSummary = new List<BootcampDto.ChestHistory>();
            response.ThighSummary = new List<BootcampDto.ThighHistory>();

            measurements.ForEach(measurement =>
            {
                if (measurement.Biceps != 0)
                {
                    response.BicepsSummary.Add(new BootcampDto.BicepsHistory { Measurement = measurement.Biceps, LogDate = measurement.CreatedOn });
                }
                if (measurement.Waist != 0)
                {
                    response.WaistSummary.Add(new BootcampDto.WaistHistory { Measurement = measurement.Waist, LogDate = measurement.CreatedOn });
                }
                if (measurement.Weight != 0)
                {
                    response.WeightSummary.Add(new BootcampDto.WeightlossHistory { Weight = measurement.Weight, LogDate = measurement.CreatedOn });
                }
                if (measurement.Hips != 0)
                {
                    response.HipsSummary.Add(new BootcampDto.HipsHistory { Measurement = measurement.Biceps, LogDate = measurement.CreatedOn });
                }
                if (measurement.Chest != 0)
                {
                    response.ChestSummary.Add(new BootcampDto.ChestHistory { Measurement = measurement.Waist, LogDate = measurement.CreatedOn });
                }
                if (measurement.Thighs != 0)
                {
                    response.ThighSummary.Add(new BootcampDto.ThighHistory { Measurement = measurement.Weight, LogDate = measurement.CreatedOn });
                }
            });

            return Request.CreateResponse<BootcampDto.CompletedBootcampResponse>(HttpStatusCode.OK, response);
        }

        [HttpPost]
        [ActionName("CreateBootCamp")]
        [ResponseType(typeof(BootcampDto.BootCampCreateResponse))]
        [Authorize]
        public HttpResponseMessage CreateBootCamp(BootcampDto.CreateBootCampRequest request)
        {
            HttpResponseMessage response = null;

            if (!ModelState.IsValid)
            {
                return response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest, 
                    new BootcampDto.ErrorResponse(ErrorHelpers.ModelStateErrors(ModelState)));
            }

            if(request.StartDate.Date <= DateTime.Now.Date)
            {
                return response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest,
                    new BootcampDto.ErrorResponse(Resources.StringResources.BootCampStartDayError));
            }

            DateTime timeStamp = DateTime.Now;

            ClaimsPrincipal currentClaimsPrincipal = (ClaimsPrincipal)HttpContext.Current.User;

            Claim claim = currentClaimsPrincipal.FindFirst(ClaimTypes.SerialNumber);
            int userId = int.Parse(claim.Value);

            claim = currentClaimsPrincipal.FindFirst(ClaimTypes.Email);
            string email = claim.Value;
            try
            {
                Bootcamp.BootCampUser bootCampUser = uow.Repository<Bootcamp.BootCampUser>().FindById(userId);
                if (bootCampUser.AmountPaid.HasValue)
                {
                    int count = uow.Repository<Bootcamp.BootCamp>()
                    .Query()
                    .Filter(o => (o.CreatorId == bootCampUser.Id) &&
                    (request.StartDate >= o.StartDate && request.StartDate <= o.EndDate && o.BootCampStatus==BootCampStatus.Active))
                    .Get().Count();

                    if (count > 0)
                    {
                        return Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.BadRequest, new BootcampDto.ErrorResponse(Resources.StringResources.BootcampError1));
                    }
                    else
                    {
                        Bootcamp.BootCamp bootCamp = new Bootcamp.BootCamp
                        {
                            Name = request.Name,
                            About = request.About,
                            BootcampImage = request.ImageUrl,
                            CreatedOn = timeStamp,
                            UpdatedOn = timeStamp,
                            StartDate = request.StartDate.Date,
                            EndDate = request.StartDate.Date.AddDays(6 * 7),
                            DroppedOff = 0,
                            MaxMembers = MAXMEMBERS,
                            SignedUp = 1,
                            IsPrivate = request.IsPrivate,
                            CreatorId = bootCampUser.Id,
                            Owner = bootCampUser,
                            State = ObjectInterfaces.ObjectState.Added
                        };
                        Bootcamp.BootCampUserDetail BootCampUserDetail = new Bootcamp.BootCampUserDetail();
                        BootCampUserDetail.BootCamp = bootCamp;
                        BootCampUserDetail.CreatedOn = DateTime.UtcNow;
                        BootCampUserDetail.UpdatedOn = DateTime.UtcNow;
                        BootCampUserDetail.UserId = bootCampUser.Id;
                        BootCampUserDetail.JoinDate = DateTime.UtcNow;
                        BootCampUserDetail.DroppedOut = false;
                        BootCampUserDetail.State = ObjectInterfaces.ObjectState.Added;
                        if(bootCamp.BootCampUserDetails == null)
                        {
                            bootCamp.BootCampUserDetails = new List<Bootcamp.BootCampUserDetail>();
                        }
                        bootCamp.BootCampUserDetails.Add(BootCampUserDetail);
                        if (request.InvitedUser != null && request.InvitedUser.Count > 0 && bootCamp.BootCampInvitedUsers == null)
                        {
                            bootCamp.BootCampInvitedUsers = new List<Bootcamp.BootCampInvitedUsers>();
                            bootCamp.UserNotifications = new List<UserNotification>();

                            request.InvitedUser.ForEach((user) =>
                            {
                                Bootcamp.BootCampInvitedUsers invitedUser = new Bootcamp.BootCampInvitedUsers();
                                UserNotification userNotification = new UserNotification();

                                userNotification.NotificationType = NotificationType.JoinBootCamp;
                                userNotification.ForUserId = user.Id;
                                userNotification.SentByUserId = userId;
                                userNotification.IsDeleted = false;
                                userNotification.BootCamp = bootCamp;
                                userNotification.CreatedOn = timeStamp;
                                userNotification.UpdatedOn = timeStamp;
                                userNotification.Title = bootCampUser.FullName;
                                userNotification.TextLine1 = "You have been invited to the Boot Camp: ";
                                userNotification.TextLine2 = request.Name;
                                userNotification.State = ObjectInterfaces.ObjectState.Added;
                                bootCamp.UserNotifications.Add(userNotification);

                                invitedUser.UserId = user.Id;
                                invitedUser.BootCamp = bootCamp;
                                invitedUser.State = ObjectInterfaces.ObjectState.Added;
                                bootCamp.BootCampInvitedUsers.Add(invitedUser);
                            });
                        }

                        uow.Repository<Bootcamp.BootCamp>().InsertGraph(bootCamp);

                        uow.Save();
                        return Request.CreateResponse<BootcampDto.BootCampCreateResponse>(HttpStatusCode.OK, new BootcampDto.BootCampCreateResponse { Id = bootCamp.Id, Message = Resources.StringResources.BootCampCreated });
                    }
                }
                else
                {
                    return Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.MethodNotAllowed, new BootcampDto.ErrorResponse("Only paid members can start a bootcamp."));
                }
            }
            catch(Exception ex)
            {
                logger.Log<Exception>(LogLevel.Error, ex);
                return response = Request.CreateResponse<BootcampDto.ErrorResponse>(HttpStatusCode.InternalServerError,
                    new BootcampDto.ErrorResponse(Resources.StringResources.Exception));
            }
        }
    }
}
