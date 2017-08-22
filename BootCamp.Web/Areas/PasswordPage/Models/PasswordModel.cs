using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BootCamp.Web.Areas.PasswordPage.Models
{
    public class PasswordModel
    {
        [Required]
        [RegularExpression( "^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{6,}$",ErrorMessage= "Must contain at least 6 characters and 1 number")]
        public string Password { get; set; }
        public string ReEnterPassword { get; set; }
        public string AuthKey { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }
    }
}