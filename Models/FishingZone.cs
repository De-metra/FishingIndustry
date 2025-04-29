namespace FishingIndustry.Models
{
    public class FishingZone
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        public double Latitude { get; set; }         // Широта
        public double Longitude { get; set; }        // Долгота

        public ICollection<FishType> FishTypes { get; set; } = new List<FishType>(); // Список рыб
    }
}
