using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FishingIndustry.ViewModels
{
    public class FishingZoneViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Фото")]
        public IFormFile ImageUpload { get; set; }

        [Display(Name = "Широта")]
        public double Latitude { get; set; }

        [Display(Name = "Долгота")]
        public double Longitude { get; set; }

        [Display(Name = "Рыбы, водящиеся здесь")]
        public List<int> SelectedFishTypeIds { get; set; } = new List<int>();

        public List<SelectListItem> AvailableFishTypes { get; set; } = new List<SelectListItem>();
    }

}
