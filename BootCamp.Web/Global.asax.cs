using BootCamp.Models;
using BootCamp.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Tracing;
using System.Web.Mvc;
using System.Web.Routing;
using System.Configuration;
using BootCamp.Web.scheduler;

namespace BootCamp.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            //GlobalConfiguration.Configuration.Services.Replace(typeof(ITraceWriter), new BootCampLogger());
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            BootCampScheduler.Start();
            //Hangfire.GlobalConfiguration.Configuration.UseMySqlStorage("hangfire");
            //AutoMapperConfiguration.Configure();

        }
        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}
