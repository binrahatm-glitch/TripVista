using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripVista.Models.Entities
{
    public class Bookings
    {
        [Key]
        public Guid BookingId { get; set; }

        public Guid UserId { get; set; }
        public Guid PackageId { get; set; }

        public int TravelersCount { get; set; } = 1;
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public DateTime? TravelDate { get; set; }

        public decimal? TotalPrice { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Refunded
        public string PaymentMethod { get; set; } = "None"; // Card, Cash, Online

        // Navigation
        public Users? User { get; set; }
        public Packages? Package { get; set; }
    }
}
