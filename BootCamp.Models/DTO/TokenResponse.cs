using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class TokenResponse:SuccessResponse
    {
        public TokenResponse()
        {

        }
        public TokenResponse(string message,string token):base(message)
        {
            this.Token = token;
        }
        public string Token { get; set;}
    }
}
