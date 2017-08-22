using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class UserResponse
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string ProfilePicture { get; set; }
        public bool Public { get; set; }
        public int CurrentBootCampId { get; set; }
        public DateTime CurrentBootCampStartDate { get; set; }
        public DateTime CurrentBootCampEndDate { get; set; }
        public UserVitalStatistics Statistics { get; set; }
        public List<MeasurementDto> Weight { get; set; }
        public List<MeasurementDto> Chest { get; set; }
        public List<MeasurementDto> Bicep { get; set; }
        public List<MeasurementDto> Waist { get; set; }
        public List<BodyPicture> BodyPictures { get; set; }
        public List<BootCampHistoryDto> BootCamps { get; set; }
        public BootCampHistoryDto CurrentBootCamp { get; set; }
    }

    public class BodyPicture
    {
        public DateTime PictureDate { get; set; }
        public string ImageName { get; set; }
    }

    public class MeasurementDto
    {
        public int Value { get; set; }
        public DateTime Date { get; set; }
    }

    public class BootCampHistoryDto
    {
        public int BootCampId { get; set; }
        public string BootCampName { get; set; }
        public string BootCampImage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalMembers { get; set; }
        public int MaxMemebers { get; set; }
    }
}
