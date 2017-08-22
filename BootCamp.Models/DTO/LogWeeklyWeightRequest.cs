using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class LogWeeklyWeightRequest
    {
        public int BootCampId { get; set; }
        public int WeekNo { get; set; }
        public int Weight { get; set; }
    }
    public class LogWeeklyMeasurementRequest
    {
        public int BootCampId { get; set; }
        public int WeekNo { get; set; }
        public int Chest { get; set; }
        public int Biceps { get; set; }
        public int Waist { get; set; }
        public int Hip { get; set; }
        public int Thigh { get; set; }
    }

    public class LogWeeklyBodyImagesRequest
    {
        public int BootCampId { get; set; }
        public int WeekNo { get; set; }
        public List<string> Images { get; set; }
    }

    public class LogWeeklyGroceryRequest
    {
        public int BootCampId { get; set; }
        public int WeekNo { get; set; }
    }
}
