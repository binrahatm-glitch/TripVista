using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripVista.Models.Entities
{
    public class Packages
    {
        [Key]
        public Guid PackageId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? ImagePath { get; set; }
        public decimal? Price { get; set; }

        public decimal Discount { get; set; } = 0;
        public int? DurationDays { get; set; }

        public string? Details { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public IEnumerable<PackageDestinations> PackageDestinations { get; set; } = new List<PackageDestinations>();
        public IEnumerable<Bookings>? Bookings { get; set; }
    }
}
