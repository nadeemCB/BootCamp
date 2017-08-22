using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class LoginResponse:SuccessResponse
    {
        public LoginResponse() { }
        public LoginResponse(string message, string token,string imageUrl,bool profileCreated,int userId):base(message)
        {
            this.Token = token;
            this.ImageUrl = imageUrl;
            this.ProfileCreated = profileCreated;
            this.UserId = userId;
        }
        public bool ProfileCreated { get; set; }
        public string Token { get; set; }
        public string ImageUrl { get; set; }
        public int UserId { get; set; }
    }
}
