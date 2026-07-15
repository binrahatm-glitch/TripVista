using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class AddBookingViewModel
    {
        public Guid PackageId { get; set; }
        public string PackageTitle { get; set; } = string.Empty;
        public decimal PricePerPerson { get; set; }
        public int MaxGroupSize { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "At least 1 traveler is required.")]
        public int NumberOfTravelers { get; set; } = 1;
    }
}
