using BootCamp.Models.DTO;
using BootCamp.Web.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;

namespace BootCamp.Web.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                actionContext.Response = actionContext.Request.CreateResponse<ErrorResponse>
                    (HttpStatusCode.BadRequest, new ErrorResponse(ErrorHelpers.ModelStateErrors(actionContext.ModelState)));
            }
        }
    }
}