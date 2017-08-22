using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class CreateBootCampRequest
    {
        [StringLength(100, ErrorMessageResourceName = "BootCampNameLengthError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public string Name { get; set; }
        [StringLength(250, ErrorMessageResourceName = "BootCampAboutLengthError", ErrorMessageResourceType = typeof(Resources.StringResources))]
        public string About { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public bool IsPrivate { get; set; }
        public List<UserContactsResponseDto> InvitedUser { get; set; }
    }

    public class RestartBootCampRequest
    {
        [Required]
        public int BootCampId { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
    }
}
