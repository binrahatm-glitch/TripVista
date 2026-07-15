using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class CreatePollViewModel
    {
        [Required, MaxLength(300)]
        public string Question { get; set; } = string.Empty;

        [MinLength(2, ErrorMessage = "A poll needs at least 2 options.")]
        public List<string> Options { get; set; } = new() { "", "" };
    }
}
