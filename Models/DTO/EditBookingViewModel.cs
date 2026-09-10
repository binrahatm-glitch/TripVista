using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class EditBookingViewModel
    {
        public Guid BookingId { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string PackageTitle { get; set; } = string.Empty;
        public decimal PricePerPerson { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "At least 1 traveler is required.")]
        public int TravelersCount { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Total price cannot be negative.")]
        public decimal? TotalPrice { get; set; }
    }
}
