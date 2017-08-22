using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class EditBootCamp
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string ImageUrl { get; set; }
    }
}
