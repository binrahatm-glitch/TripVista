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

        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerPerson { get; set; }

        public int DurationDays { get; set; }

        public int MaxGroupSize { get; set; }

        public DateTime TourStartDate { get; set; }

        public DateTime TourEndDate { get; set; }

        public string? Details { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid CreatedByAdminId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Users? CreatedByAdmin { get; set; }
        public IEnumerable<Bookings>? Bookings { get; set; }
        public ICollection<PackageDestinations>? PackageDestinations { get; set; }
    }
}
