using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class ChangeEmailRequest
    {
        [StringLength(300, ErrorMessageResourceName = "LengthError500", ErrorMessageResourceType = typeof(Resources.StringResources)),
            Required(AllowEmptyStrings = false, ErrorMessageResourceName = "EmailMandatoryError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        [EmailAddress(ErrorMessageResourceName = "EmailInvalidError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public string OldEmail { get; set; }
        [StringLength(300, ErrorMessageResourceName = "LengthError500", ErrorMessageResourceType = typeof(Resources.StringResources)),
            Required(AllowEmptyStrings = false, ErrorMessageResourceName = "EmailMandatoryError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        [EmailAddress(ErrorMessageResourceName = "EmailInvalidError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public string NewEmail { get; set; }
    }
}
