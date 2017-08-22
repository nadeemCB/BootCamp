using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class LeaveBootCampRequest
    {
        [Required]
        [Range(1,int.MaxValue, ErrorMessage ="Bootcamp not found")]
        public int BootCampId { get; set; }
    }
}
