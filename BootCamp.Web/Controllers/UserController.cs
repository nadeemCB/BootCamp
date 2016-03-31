using BootCamp.Core;
using BootCamp.Core.BoundedContext;
using BootCamp.DomainObjects;
using BootCamp.Helper;
using BootCamp.Models.DTO;
using BootCamp.Utils;
using BootCamp.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace BootCamp.Web.Controllers
{
    public class UserController : ApiController
    {

        private readonly IUnitOfWork uow;
        public UserController()
        {
            uow = new UnitOfWork<UserContext>();
        }

        [HttpGet]
        [ActionName("GetUser")]
        public HttpResponseMessage GetUser()
        {
            //User user = new User
            //{
            //    FirstName = "Test",
            //    LastName = "test",
            //    Email = "test@test.com",
            //    Password = EncryptionHelper.EncryptString("Nadeem"),
            //    PhoneNumber = "23 23 234354",
            //    State = ObjectInterfaces.ObjectState.Added
            //};
            //uow.Repository<User>().Insert(user);

            //int id = uow.Save();
            User user = uow.Repository<User>().FindById(4);
            user.Password = EncryptionHelper.DecryptString(user.Password);
            HttpResponseMessage response = Request.CreateResponse<User>(HttpStatusCode.OK, user);

            return response;
        }

        [HttpPost]
        [ActionName("RegisterUser")]
        public HttpResponseMessage RegisterUser(UserRegisteration userRegisteration)
        {
            HttpResponseMessage response = null;
            if (ModelState.IsValid)
            {
                User user = uow.Repository<User>().Query().Filter(o => o.Email.ToLower().Trim().Equals(userRegisteration.Email.ToLower().Trim())).Get().FirstOrDefault();
                if(user == null)
                {
                    user = new DomainObjects.User
                    {
                        Email = userRegisteration.Email.Trim(),
                        Password = EncryptionHelper.EncryptString(userRegisteration.Password.Trim()),
                        PhoneNumber = userRegisteration.PhoneNumber.Trim(),
                        Verified = false
                    };
                    uow.Repository<User>().Insert(user);
                    uow.Save();
                    response = Request.CreateResponse(HttpStatusCode.OK, Resources.StringResources.AccountCreated);
                }
                else if(user.Verified == true)
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest,Resources.StringResources.EmailExistsError);
                }
                else if (user.Verified == false)
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, Resources.StringResources.AccountVerifyError);
                }
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, ModelStateHelpers.ModelStateErrors(ModelState));
            }
            return response;
        }
        [HttpPost]
        [ActionName("LoginUser")]
        public HttpResponseMessage LoginUser(UserLogin userLogin)
        {
            HttpResponseMessage response = null;
            if (ModelState.IsValid)
            {
                User user = uow.Repository<User>().Query().Filter(o => o.Email.Trim().ToLower().Equals(userLogin.Email.ToLower().Trim())).Get().FirstOrDefault();
                if (user != null && EncryptionHelper.DecryptString(user.Password).ToLower().Trim().Equals(userLogin.Password.Trim().ToLower())
                    && user.Verified == true)
                {

                    //Map User object to DTO
                    response = Request.CreateResponse(HttpStatusCode.OK);
                }
                else if(user != null && EncryptionHelper.DecryptString(user.Password).ToLower().Trim().Equals(userLogin.Password.Trim().ToLower())
                    && user.Verified == false)
                {
                    ErrorResponse errorResponse = new ErrorResponse();
                    errorResponse.AddError(Resources.StringResources.AccountVerifyError);
                    response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, errorResponse);
                }
                else
                {
                    ErrorResponse errorResponse = new ErrorResponse();
                    errorResponse.AddError(Resources.StringResources.LoginCredentialsError);
                    response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, errorResponse);
                }
            }
            else
            {
                ErrorResponse errorResponse = new ErrorResponse();
                errorResponse.AddErrors(ModelStateHelpers.ModelStateErrors(ModelState));
                response = Request.CreateResponse<ErrorResponse>(HttpStatusCode.BadRequest, errorResponse);
            }
            return response;
        }
        [HttpPost]
        [ActionName("ForgotPassword")]
        public HttpResponseMessage ForgotPassword(ForgotPassword forgotPassword)
        {
            HttpResponseMessage response = null;
            if (ModelState.IsValid)
            {
                User user = uow.Repository<User>().Query().Filter(o => o.Email.ToLower().Trim().Equals(forgotPassword.Email.ToLower().Trim())).Get().FirstOrDefault();
                if (user != null)
                {
                    string body = string.Empty;
                    using (StreamReader sr = new StreamReader(HostingEnvironment.MapPath("\\App_Data\\Templates\\"+ ConfigurationManager.AppSettings["ForgotPasswordTemplate"] + "")))
                    {
                        body = sr.ReadToEnd();
                    }
                    string name = HttpUtility.UrlEncode(user.FullName);
                    string email = HttpUtility.UrlEncode(user.Email);
                    string password = HttpUtility.UrlEncode(user.Password);
                    string messageBody = string.Format(body, name, email, password);

                    MailHelper mailHelper = new MailHelper
                    {
                        Body = messageBody,
                        Recipient = "",
                        RecipientCC = null,
                        Subject = @"Forgot Password"
                    };
                    mailHelper.Send();

                    response = Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, Resources.StringResources.EmailNotExistError);
                }
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, ModelStateHelpers.ModelStateErrors(ModelState));
            }
            return response;
        }
    }
}
