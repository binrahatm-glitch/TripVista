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

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalPrice { get; set; }

        public string Status { get; set; } = "PendingPayment"; // PendingPayment, Confirmed, Cancelled, Completed
        public bool AreTravelerDetailsSubmitted { get; set; } = false;
        public DateTime? CancelledAt { get; set; }

        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid
        public string? PaymentReference { get; set; }
        public DateTime? PaidAt { get; set; }

        // Navigation
        public Users? User { get; set; }
        public Packages? Package { get; set; }
        public ICollection<BookingTravelers>? BookingTravelers { get; set; }
    }
}
