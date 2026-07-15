using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TripVista.Models.Entities
{
    public class Users
    {
        [Key]
        public Guid UserId { get; set; }

        [MaxLength(100)]
        public string? FullName { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "user"; // user/admin

        public bool IsEmailVerified { get; set; } = false;

        public string? AvatarPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Bookings>? Bookings { get; set; }
        public ICollection<Destinations>? SubmittedDestinations { get; set; }
    }
}
