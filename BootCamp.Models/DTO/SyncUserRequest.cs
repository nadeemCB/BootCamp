using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class SyncUserRequest
    {
        public List<UserContactsDto> UserContacts { get; set; }
    }
}
