using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.Entities
{
    public class BookingTravelers
    {
        [Key]
        public Guid Id { get; set; }

        public Guid BookingId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public int Age { get; set; }

        [Required, MaxLength(50)]
        public string CNICOrPassportNumber { get; set; } = string.Empty;

        // Navigation
        public Bookings? Booking { get; set; }
    }
}
