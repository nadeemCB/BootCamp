﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class UserRegisteration
    {
        [StringLength(300, ErrorMessageResourceName = "LengthError500", ErrorMessageResourceType = typeof(Resources.StringResources)),
            Required(AllowEmptyStrings = false, ErrorMessageResourceName = "EmailMandatoryError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        [EmailAddress(ErrorMessageResourceName = "EmailInvalidError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public string Email { get; set; }
        [StringLength(20,ErrorMessageResourceName = "PasswordLengthError", ErrorMessageResourceType =typeof(Resources.StringResources),MinimumLength =6)]
        public string Password { get; set; }
        [Required]
        [Phone(ErrorMessageResourceName ="PhoneNoError",ErrorMessageResourceType =typeof(Resources.StringResources))]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid number")]
        public string PhoneNumber { get; set; }
    }
}