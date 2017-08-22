using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class GroceryListResponse
    {
        public List<ItemCategoryDto> ItemCategories { get; set; }
    }

    public class ItemCategoryDto
    {
        public string ItemCategory { get; set; }
        public List<CategoryItemDto> CategoryItems { get; set; }
    }

    public class CategoryItemDto
    {
        public string Name { get; set; }
    }
}
