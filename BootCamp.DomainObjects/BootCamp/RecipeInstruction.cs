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
    [Table("bc_recipe_instruction")]
    public class RecipeInstruction : IObjectState
    {
        [Key]
        public int Id { get; set; }
        public int RecipeId { get; set; }
        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; set; }
        public string Instruction { get; set; }
        public int Sequence { get; set; }
        [NotMapped]
        public ObjectState State
        {
            get;set;
        }
    }
}
