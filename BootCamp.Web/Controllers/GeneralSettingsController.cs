using BootCamp.Models.DTO;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Description;

namespace BootCamp.Web.Controllers
{
    public class GeneralSettingsController : ApiController
    {
        [HttpGet]
        [ActionName("GetPrivacyPolicy")]
        [ResponseType(typeof(SuccessResponse))]
        public HttpResponseMessage GetPrivacyPolicy()
        {
            HttpResponseMessage response = null;
            SuccessResponse successResponse = new SuccessResponse();
            string body = string.Empty;
            using (StreamReader sr = new StreamReader(HostingEnvironment.MapPath("\\App_Data\\Templates\\" + ConfigurationManager.AppSettings["PrivacyTemplate"] + "")))
            {
                successResponse.Message = sr.ReadToEnd();
            }
            response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, successResponse);
            return response;
        }

        [HttpGet]
        [ActionName("GetTerms")]
        [ResponseType(typeof(SuccessResponse))]
        public HttpResponseMessage GetTerms()
        {
            HttpResponseMessage response = null;
            SuccessResponse successResponse = new SuccessResponse();
            string body = string.Empty;
            using (StreamReader sr = new StreamReader(HostingEnvironment.MapPath("\\App_Data\\Templates\\" + ConfigurationManager.AppSettings["TermsTemplate"] + "")))
            {
                successResponse.Message = sr.ReadToEnd();
            }
            response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, successResponse);
            return response;
        }

        [HttpGet]
        [ActionName("GetGeneralSettings")]
        [ResponseType(typeof(GeneralSettingsResponse))]
        public HttpResponseMessage GetGeneralSettings()
        {
            HttpResponseMessage response = null;
            GeneralSettingsResponse settingResponse = new GeneralSettingsResponse();
            string body = string.Empty;
            string version = ConfigurationManager.AppSettings["iOSVersion"];
            settingResponse.AppVersion = version;
            settingResponse.Critical = Convert.ToBoolean(ConfigurationManager.AppSettings["Cirtical"]);
            settingResponse.S3 = ConfigurationManager.AppSettings["s3"];
            response = Request.CreateResponse<SuccessResponse>(HttpStatusCode.OK, settingResponse);
            return response;
        }

    }
}
