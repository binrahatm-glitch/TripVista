using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [MaxLength(100)]
        public string Password { get; set; } = null!;
    }
}
