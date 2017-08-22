using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class NewPasswordResponse:SuccessResponse
    {
        public string EmailAddress { get; set; }
    }
}
