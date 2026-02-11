using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripVista.Models.Entities
{
    public class Destinations
    {
        [Key]
        public Guid DestinationId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public string? Description { get; set; }
        public string? ImagePath { get; set; }

        public string? BestTimeToVisit { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public decimal? AverageCostMin { get; set; }
        public decimal? AverageCostMax { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<PackageDestinations>? PackageDestinations { get; set; }
        public ICollection<Reviews>? Reviews { get; set; }
    }
}
