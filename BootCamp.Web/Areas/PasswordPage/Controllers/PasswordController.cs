using BootCamp.Core;
using BootCamp.Core.BoundedContext;
using BootCamp.DomainObjects;
using BootCamp.Utils;
using BootCamp.Web.Areas.PasswordPage.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Security.Tokens;
using System.Web;
using System.Web.Mvc;

namespace BootCamp.Web.Areas.PasswordPage.Controllers
{
    public class PasswordController : Controller
    {
        private readonly IUnitOfWork uow;
        public PasswordController()
        {
            uow = new UnitOfWork<UserContext>();

        }
        // GET: PasswordPage/Password
        public ActionResult Index(string key)
        {
            PasswordModel model = new PasswordModel();
            model.HasError = false;
            model.AuthKey = key; 
            return View("Index", model);
        }
        [HttpPost]
        public ActionResult ChangePassword(PasswordModel formData)
        {
            PasswordModel model = new PasswordModel();
            model.AuthKey = formData.AuthKey;
            model.Password = formData.Password;
            model.ReEnterPassword = formData.ReEnterPassword;
            if (!ModelState.IsValid)
            {
                model.HasError = true;
                model.ErrorMessage = "Must contain at least 6 characters and 1 number.";
                return View("Index", model);
            }
            else if(string.IsNullOrEmpty(model.ReEnterPassword) || !formData.Password.Trim().Equals(formData.ReEnterPassword.Trim()))
            {
                //Passwords do not match.
                model.HasError = true;
                model.ErrorMessage = "Passwords do not match.";
                return View("Index", model);
            }

            model.AuthKey = formData.AuthKey;
            model.Password = formData.Password;
            model.ReEnterPassword = formData.ReEnterPassword;
            try
            {
                string jwt = formData.AuthKey;

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                TokenValidationParameters parms = new TokenValidationParameters()
                {
                    ValidAudience = ConfigurationManager.AppSettings.Get("Audiance"),
                    ValidateAudience = true,
                    ValidIssuers = new List<string>() { ConfigurationManager.AppSettings.Get("ISSUER") },
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    IssuerSigningToken = new BinarySecretSecurityToken(Convert.FromBase64String(ConfigurationManager.AppSettings.Get("SecurityKey")))
                };
                SecurityToken validatedToken;

                ClaimsPrincipal principal = tokenHandler.ValidateToken(jwt, parms, out validatedToken);
                Claim claim = principal.FindFirst(ClaimTypes.SerialNumber);

                int userId = int.Parse(claim.Value);

                User user = uow.Repository<User>().FindById(userId);

                user.Password = EncryptionHelper.EncryptString(formData.Password);
                user.UpdatedOn = DateTime.UtcNow;
                user.State = ObjectInterfaces.ObjectState.Modified;
                uow.Repository<User>().Update(user);
                uow.Save();
                return View("Success");
                
            }
            catch (SecurityTokenExpiredException)
            {
                model.HasError = true;
                model.ErrorMessage = "Token has expired. Initiate a new request for forgot password.";
            }
            catch(Exception)
            {
                model.HasError = true;
                model.ErrorMessage = "Something went wrong. Initiate a new request for forgot password.";
            }

            return View("Index", model);
        }
    }
}