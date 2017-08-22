using BootCamp.ObjectInterfaces;
using BootCamp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.DomainObjects.BootCamp
{
    [Table("bc_user_favorite_recipe")]
    public class FavoriteRecipe: IObjectState
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public BootCampUser User { get; set; }
        public int RecipeId { get; set; }
        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }
        public MarkAs Favorite { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get; set;
        }
    }
}
