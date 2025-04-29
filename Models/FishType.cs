using System.ComponentModel.DataAnnotations;

namespace FishingIndustry.Models
{
    public class FishType
    {
        public int Id { get; set; }

        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Цена за кг")]
        public decimal AveragePricePerKg { get; set; }

        [Display(Name = "Фото")]
        public string ImagePath { get; set; }

        [Display(Name = "Семейство")]
        public string Family { get; set; }                   // Семейство рыбы

        [Display(Name = "Место обитания")]
        public string HabitatType { get; set; }              

        [Display(Name = "Водоёмы")]
        public ICollection<FishingZone> FishingZones { get; set; } = new List<FishingZone>(); // Где водится
    }
}
