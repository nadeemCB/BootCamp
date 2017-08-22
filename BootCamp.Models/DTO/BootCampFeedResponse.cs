using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class BootCampFeedResponse
    {
        public int TotalRecords { get; set; }
        public FeedData Data { get; set; }
    }

    public class FeedData
    {
        public List<FeedDto> Feed { get; set; }
    }
    public class FeedDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public double MinutesAgo { get; set; }
        public DateTime ActivityTime { get; set; }
        public string Text { get; set; }
        public string UserImage { get; set; }
        public string Image { get; set; }
        public LogType LogType { get; set; }
        public Nullable<MealType> MealType { get; set; }
        public ActivityStatus Status { get; set; }
    }
}
