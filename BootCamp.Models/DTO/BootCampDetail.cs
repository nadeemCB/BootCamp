using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class BootCampDetail:Bootcamp
    {
        public BootCampUser Owner { get; set; }
        public List<BootCampUser> Groupies { get; set; }
        public List<BootCampUser> InvitedGroupies { get; set; }
    }
}
