using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TripVista.Models.DTO
{
    public class EditDestinationViewModel
    {
        public Guid DestinationId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Location { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? Image { get; set; }
    }
}
