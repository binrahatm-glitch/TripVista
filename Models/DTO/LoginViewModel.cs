using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class LoginViewModel
    {
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
