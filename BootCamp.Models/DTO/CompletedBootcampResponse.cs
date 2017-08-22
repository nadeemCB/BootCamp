using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class CompletedBootcampResponse
    {
        public int BootCampId { get; set; }
        public string BootcampImage { get; set; }
        public string BootcampName { get; set; }
        public DateTime CompletionDate { get; set; }
        public DateTime StartDate { get; set; }
        public WorkoutRecap WorkoutRecap { get; set; }
        public List<WeightlossHistory> WeightSummary { get; set; }
        public List<BicepsHistory> BicepsSummary { get; set; }
        public List<WaistHistory> WaistSummary { get; set; }
        public List<ChestHistory> ChestSummary { get; set; }
        public List<ThighHistory> ThighSummary { get; set; }
        public List<HipsHistory> HipsSummary { get; set; }
        public List<BodyPictures> BodyPictures { get; set; }
    }

    public class HipsHistory
    {
        public DateTime LogDate { get; set; }
        public int Measurement { get; set; }
    }

    public class ThighHistory
    {
        public DateTime LogDate { get; set; }
        public int Measurement { get; set; }
    }

    public class ChestHistory
    {
        public DateTime LogDate { get; set; }
        public int Measurement { get; set; }
    }

    public class BodyPictures
    {
        public DateTime ImageDate { get; set; }
        public string Images { get; set; }
    }

    public class WaistHistory
    {
        public DateTime LogDate { get; set; }
        public int Measurement { get; set; }
    }

    public class BicepsHistory
    {
        public DateTime LogDate { get; set; }
        public int Measurement { get; set; }
    }

    public class WeightlossHistory
    {
        public DateTime LogDate { get; set; }
        public int Weight { get; set; }
    }

    public class WorkoutRecap
    {
        public int BeginnerWorkout { get; set; }
        public int IntermediateWorkout { get; set; }
        public int ExpertWorkout { get; set; }
        public int HomeWorkout { get; set; }
        public int GymWorkout { get; set; }
    }

}
