namespace TripVista.Models.DTO
{
    public class AddDestinationDTO
    {
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
        public string? BestTimeToVisit { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? AverageCostMin { get; set; }
        public decimal? AverageCostMax { get; set; }
    }
}
