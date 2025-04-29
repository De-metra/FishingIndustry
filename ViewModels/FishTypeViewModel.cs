using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FishingIndustry.ViewModels
{
    public class FishTypeViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Display(Name = "Цена за кг")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
        public decimal AveragePricePerKg { get; set; }

        [Display(Name = "Семейство")]
        public string Family { get; set; }

        [Display(Name = "Среда обитания")]
        public string HabitatType { get; set; } 

        [Display(Name = "Фотография")]
        public IFormFile? ImageUpload { get; set; }

        [Display(Name = "Водоёмы")]
        public List<int> SelectedZoneIds { get; set; }

        // Все доступные зоны (для вывода чекбоксов)
        public List<SelectListItem> AllFishingZones { get; set; } = new();

    }

   
}
