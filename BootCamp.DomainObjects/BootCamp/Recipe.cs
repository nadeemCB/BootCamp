using BootCamp.ObjectInterfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.BootCamp
{
    [Table("bc_recipe")]
    public class Recipe : IObjectState
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public Nullable<int> Preptime { get; set; }
        public Nullable<int> Cooktime { get; set; }
        public Nullable<int> Readintime { get; set; }
        public virtual IList<RecipeIngredient> RecipeIngredients { get; set; }
        public virtual IList<FavoriteRecipe> FavoriteRecipes { get; set; }
        public virtual IList<RecipeInstruction> RecipeInstructions { get; set; }
        public virtual IList<Meal> Meals { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get; set;
        }
    }
}
