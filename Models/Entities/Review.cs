using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripVista.Models.Entities
{
    public class Reviews
    {
        [Key]
        public Guid ReviewId { get; set; }

        public Guid UserId { get; set; }
        public Guid DestinationId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }

        [MaxLength(10)]
        public string? Sentiment { get; set; } // Positive, Negative, Neutral

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Users? User { get; set; }
        public Destinations? Destination { get; set; }
    }
}
