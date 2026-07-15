using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.Entities
{
    public class PollOptions
    {
        [Key]
        public Guid PollOptionId { get; set; }

        public Guid PollId { get; set; }

        [Required, MaxLength(200)]
        public string OptionText { get; set; } = string.Empty;

        // Navigation
        public Polls? Poll { get; set; }
        public ICollection<PollVotes>? Votes { get; set; }
    }
}
