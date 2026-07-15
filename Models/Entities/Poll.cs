using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.Entities
{
    public class Polls
    {
        [Key]
        public Guid PollId { get; set; }

        [Required, MaxLength(300)]
        public string Question { get; set; } = string.Empty;

        public Guid CreatedByAdminId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public Users? CreatedByAdmin { get; set; }
        public ICollection<PollOptions>? Options { get; set; }
        public ICollection<PollVotes>? Votes { get; set; }
    }
}
