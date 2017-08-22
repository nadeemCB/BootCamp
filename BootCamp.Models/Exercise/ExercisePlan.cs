using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.Exercise
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "weeks")]
    public partial class ExercisePlan
    {

        private weeksWeek[] weekField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("week")]
        public weeksWeek[] week
        {
            get
            {
                return this.weekField;
            }
            set
            {
                this.weekField = value;
            }
        }
    }
    public class WeekData
    {
        public int TotalWeeks { get; set; }
        public int CurrentWeek { get; set; }
        public weeksWeek Week{get;set;}
    }
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeek
    {

        private weeksWeekDay[] dayField;

        private int numberField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("day")]
        public weeksWeekDay[] day
        {
            get
            {
                return this.dayField;
            }
            set
            {
                this.dayField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeekDay
    {

        private weeksWeekDayWorkouts workoutsField;

        private string nameField;

        private byte sequenceField;

        /// <remarks/>
        public weeksWeekDayWorkouts workouts
        {
            get
            {
                return this.workoutsField;
            }
            set
            {
                this.workoutsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte sequence
        {
            get
            {
                return this.sequenceField;
            }
            set
            {
                this.sequenceField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeekDayWorkouts
    {
        public int ActivityLogId { get; set; }
        public ActivityStatus ActivityStatus { get; set; }

        public WorkoutLevel WorkoutLevel { get; set; }

        public WorkoutType WorkoutType { get; set; }

        private weeksWeekDayWorkoutsWorkout workoutField;

        /// <remarks/>
        public weeksWeekDayWorkoutsWorkout workout
        {
            get
            {
                return this.workoutField;
            }
            set
            {
                this.workoutField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeekDayWorkoutsWorkout
    {
        public WorkoutLevel WorkoutLevel { get; set; }

        public WorkoutType WorkoutType { get; set; }

        public int WorkoutId { get; set; }

        public int WeekNo { get; set; }

        public int WeekDayNo { get; set; }

        private string nameField;

        private object workoutdescriptionField;

        private int warmupField;

        private string descriptionField;

        private weeksWeekDayWorkoutsWorkoutExcercise[] excercisesField;

        private byte sequenceField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        public object workoutdescription
        {
            get
            {
                return this.workoutdescriptionField;
            }
            set
            {
                this.workoutdescriptionField = value;
            }
        }

        /// <remarks/>
        public int warmup
        {
            get
            {
                return this.warmupField;
            }
            set
            {
                this.warmupField = value;
            }
        }

        /// <remarks/>
        public string description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("excercise", IsNullable = false)]
        public weeksWeekDayWorkoutsWorkoutExcercise[] excercises
        {
            get
            {
                return this.excercisesField;
            }
            set
            {
                this.excercisesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte sequence
        {
            get
            {
                return this.sequenceField;
            }
            set
            {
                this.sequenceField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeekDayWorkoutsWorkoutExcercise
    {

        private string nameField;

        private string setsRepField;

        private object imageField;

        private int timeField;

        private string descriptionField;

        private byte sequenceField;

        /// <remarks/>
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        public int time
        {
            get
            {
                return this.timeField;
            }
            set
            {
                this.timeField = value;
            }
        }
        /// <remarks/>
        public string SetsRep
        {
            get
            {
                return this.setsRepField;
            }
            set
            {
                this.setsRepField = value;
            }
        }

        /// <remarks/>
        public object image
        {
            get
            {
                return this.imageField;
            }
            set
            {
                this.imageField = value;
            }
        }

        /// <remarks/>
        public string description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte sequence
        {
            get
            {
                return this.sequenceField;
            }
            set
            {
                this.sequenceField = value;
            }
        }
    }


}
