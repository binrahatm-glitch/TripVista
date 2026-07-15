using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.Entities
{
    public class PollVotes
    {
        [Key]
        public Guid PollVoteId { get; set; }

        public Guid PollId { get; set; }
        public Guid PollOptionId { get; set; }
        public Guid UserId { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.Now;

        // Navigation
        public Polls? Poll { get; set; }
        public PollOptions? PollOption { get; set; }
        public Users? User { get; set; }
    }
}
