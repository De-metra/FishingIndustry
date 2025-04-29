using System.ComponentModel.DataAnnotations;

namespace FishingIndustry.Models
{
    public class Vessel
    {
        public int Id { get; set; }

        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Регистрационный номер")]
        public string RegistrationNumber { get; set; }

        [Display(Name = "Вместимость")]
        public int Capacity { get; set; }

        [Display(Name = "Тип судна")]
        public string TypeVessel { get; set; }

        [Display(Name = "Год выпуска")]
        public int YearBuilt { get; set; }

        [Display(Name = "Фото")]
        public string ImagePath { get; set; }
    }
}
