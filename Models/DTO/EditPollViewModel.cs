using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class EditPollViewModel
    {
        public Guid PollId { get; set; }

        [Required, MaxLength(300)]
        public string Question { get; set; } = string.Empty;

        public List<EditPollOptionInput> Options { get; set; } = new();
    }

    public class EditPollOptionInput
    {
        public Guid? PollOptionId { get; set; }

        [MaxLength(200)]
        public string OptionText { get; set; } = string.Empty;

        public bool Remove { get; set; }

        public int VoteCount { get; set; }
    }
}
