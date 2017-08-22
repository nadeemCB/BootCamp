using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class BootcampListDto
    {
        public int TotalRecord { get; set; }
        public List<Bootcamp> BootCamps { get; set; }
    }
}
