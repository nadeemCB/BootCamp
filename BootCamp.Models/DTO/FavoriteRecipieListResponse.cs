using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootCamp.Models.DTO
{
    public class FavoriteRecipieListResponse
    {
        public FavoriteRecipieListData Data { get; set; }
    }

    public class FavoriteRecipieListData
    {
        public List<FavoriteRecipieDto> FavoriteBreakfastRecipieList { get; set; }
        public List<FavoriteRecipieDto> FavoriteLunchRecipieList { get; set; }
        public List<FavoriteRecipieDto> FavoriteDinnerRecipieList { get; set; }
        public List<FavoriteRecipieDto> FavoriteSnackRecipieList { get; set; }
        public List<FavoriteRecipieDto> FavoriteRecipieList { get; set; }
    }

    public class FavoriteRecipieDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
