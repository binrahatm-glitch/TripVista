using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.Entities
{
    public class Destinations
    {
        [Key]
        public Guid DestinationId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Location { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public Guid SubmittedByUserId { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public string? AdminRemarks { get; set; }

        public Guid? ReviewedByAdminId { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        // Navigation
        public Users? SubmittedByUser { get; set; }
        public Users? ReviewedByAdmin { get; set; }
        public ICollection<PackageDestinations>? PackageDestinations { get; set; }
    }
}
