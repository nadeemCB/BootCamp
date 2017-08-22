using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class ChangePasswordRequest
    {
        [StringLength(20, ErrorMessageResourceName = "PasswordLengthError", ErrorMessageResourceType = typeof(Resources.StringResources), MinimumLength = 6)]
        public string OldPassword { get; set; }
        [StringLength(20, ErrorMessageResourceName = "PasswordLengthError", ErrorMessageResourceType = typeof(Resources.StringResources), MinimumLength = 6)]
        public string NewPassword { get; set; }
        [StringLength(20, ErrorMessageResourceName = "PasswordLengthError", ErrorMessageResourceType = typeof(Resources.StringResources), MinimumLength = 6)]
        public string ConfirmNewPassword { get; set; }
    }

    public class UpdatePasswordRequest
    {
        
        [StringLength(20, ErrorMessageResourceName = "PasswordLengthError", ErrorMessageResourceType = typeof(Resources.StringResources), MinimumLength = 6)]
        public string NewPassword { get; set; }
        [StringLength(20, ErrorMessageResourceName = "PasswordLengthError", ErrorMessageResourceType = typeof(Resources.StringResources), MinimumLength = 6)]
        public string ConfirmNewPassword { get; set; }
    }
}
