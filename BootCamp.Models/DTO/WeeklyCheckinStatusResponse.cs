using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class WeeklyCheckinStatusResponse
    {
        public List<CheckInLogData> Data { get; set; }
    }

    public class CheckInLogData
    {
        public int Week { get; set; }
        public bool WeightLogged { get; set; }
        public bool MeasurementsLogged { get; set; }
        public bool BodyPicturesLogged { get; set; }
        public bool ShoppingListLogged { get; set; }
    }
}
