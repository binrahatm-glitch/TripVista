using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripVista.Models.Entities
{
    public class UserSubmissions
    {
        [Key]
        public Guid SubmissionId { get; set; }

        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [MaxLength(100)]
        public string? Location { get; set; }

        public string? Description { get; set; }
        public string? ImagePath { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public string? AdminComment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }

        // Navigation
        public Users? User { get; set; }
    }
}
