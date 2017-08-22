using System.Web.Mvc;

namespace BootCamp.Web.Areas.PasswordPage
{
    public class PasswordPageAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "PasswordPage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.MapRoute(
            //    "HelpPage_Default",
            //    "Help/{action}/{apiId}",
            //    new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });

            context.MapRoute(
                "PasswordPage_default",
                "Password/{controller}/{action}/{id}",
                new { controller = "Password", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}