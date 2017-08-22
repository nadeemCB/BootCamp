using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class UserContactsResponse:SuccessResponse
    {
        public UserContactsResponse()
        {
            this.AppUsers = new List<UserContactsResponseDto>();
            this.OtherUsers = new List<UserContactsResponseDto>();
        }
        public List<UserContactsResponseDto> AppUsers{ get; set; }
        public List<UserContactsResponseDto> OtherUsers { get; set; }
    }
}
