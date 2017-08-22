using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class BootcampsRequest
    {
        [Required]
        public int PageNumber { get; set; }
        [Required]
        public int PageSize { get; set; }
        public string BootCampName { get; set; }
    }
}
