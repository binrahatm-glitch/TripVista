using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.Entities
{
    public class OtpVerification
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(6)]
        public string OtpCode { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Purpose { get; set; } = OtpPurpose.Registration;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
