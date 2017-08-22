using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class BootCampInvite
    {
        public int BootCampId { get; set; }
        public List<UserContactsResponseDto> InvitedUser { get; set; }
    }
}
