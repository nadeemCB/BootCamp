using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class NotificationResponse:SuccessResponse
    {
        public List<Notification> Notifications { get; set; }
    }
}
