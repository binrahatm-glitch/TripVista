using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class ExistingTravelerInfo
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string CNICOrPassportNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class ManageBookingTravelersViewModel
    {
        public Guid BookingId { get; set; }
        public string PackageTitle { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Traveler count must be at least 1.")]
        public int NewCount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal? NewPrice { get; set; }

        public List<ExistingTravelerInfo> ExistingTravelers { get; set; } = new();
        public List<TravelerDetailInput> AdditionalTravelers { get; set; } = new();
    }
}
