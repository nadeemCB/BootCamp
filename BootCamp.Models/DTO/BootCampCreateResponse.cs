using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class BootCampCreateResponse: SuccessResponse
    {
        public int Id { get; set; }
        public bool IsOwner { get; set; }
        public string CampName { get; set; }
        public string CampImage { get; set; }
        public bool Started { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
