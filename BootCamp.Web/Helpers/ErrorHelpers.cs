using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;

namespace BootCamp.Web.Helpers
{
    public static class ErrorHelpers
    {
        public static List<string> ModelStateErrors(ModelStateDictionary modelState)
        {
            var errors = new List<string>();
            foreach (var state in modelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }
            return errors;
        }
        public static List<string> StringError(string error)
        {
            var errors = new List<string>();
            errors.Add(error);
            return errors;
        }
    }
}