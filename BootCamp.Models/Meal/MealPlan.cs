using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.Meal
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false,ElementName ="weeks")]
    public partial class MealPlan
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
        public weeksWeek Week { get; set; }
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

    public partial class weeksWeekDay
    {
        public DateTime Day { get; set; }
    }


    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeekDay
    {

        private weeksWeekDayMeal[] mealField;

        private string nameField;

        private int sequenceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("meal")]
        public weeksWeekDayMeal[] meal
        {
            get
            {
                return this.mealField;
            }
            set
            {
                this.mealField = value;
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
        public int sequence
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
    public partial class weeksWeekDayMeal
    {
        public ActivityStatus ActivityStatus { get; set; }

        private weeksWeekDayMealRecipe[] recipeField;

        private string typeField;

        private int sequenceField;

        public int ActivtyLogId;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("recipe")]
        public weeksWeekDayMealRecipe[] recipe
        {
            get
            {
                return this.recipeField;
            }
            set
            {
                this.recipeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int sequence
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

    public partial class weeksWeekDayMealRecipe
    {
        public int Id { get; set; }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeekDayMealRecipe
    {
        public bool IsFavorite { get; set; }
        private int preptimeField;

        private int cooktimeField;

        private int readintimeField;

        private string imageField;

        private weeksWeekDayMealRecipeDirection[] directionsField;

        private weeksWeekDayMealRecipeIngredient[] ingredientsField;

        private string nameField;

        private int sequenceField;

        private bool sequenceFieldSpecified;

        /// <remarks/>
        public int preptime
        {
            get
            {
                return this.preptimeField;
            }
            set
            {
                this.preptimeField = value;
            }
        }

        /// <remarks/>
        public int cooktime
        {
            get
            {
                return this.cooktimeField;
            }
            set
            {
                this.cooktimeField = value;
            }
        }

        /// <remarks/>
        public int readintime
        {
            get
            {
                return this.readintimeField;
            }
            set
            {
                this.readintimeField = value;
            }
        }

        /// <remarks/>
        public string image
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
        [System.Xml.Serialization.XmlArrayItemAttribute("direction", IsNullable = false)]
        public weeksWeekDayMealRecipeDirection[] directions
        {
            get
            {
                return this.directionsField;
            }
            set
            {
                this.directionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("ingredient", IsNullable = false)]
        public weeksWeekDayMealRecipeIngredient[] ingredients
        {
            get
            {
                return this.ingredientsField;
            }
            set
            {
                this.ingredientsField = value;
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
        public int sequence
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

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sequenceSpecified
        {
            get
            {
                return this.sequenceFieldSpecified;
            }
            set
            {
                this.sequenceFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeekDayMealRecipeDirection
    {

        private int sequenceField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int sequence
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

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class weeksWeekDayMealRecipeIngredient
    {

        private string nameField;

        private string measurementField;

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
        public string measurement
        {
            get
            {
                return this.measurementField;
            }
            set
            {
                this.measurementField = value;
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



