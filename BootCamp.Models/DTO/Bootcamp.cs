using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class Bootcamp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string ImageUrl { get; set; }
        public bool IsPrivate { get; set; }
        public int MemebersLimit { get; set; }
        public int RegisteredMemebers { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BootCampStatus Status { get; set; }
    }
}
