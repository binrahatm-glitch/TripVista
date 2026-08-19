using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TripVista.Models.DTO
{
    public class UpdateProfileViewModel
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? AvatarPath { get; set; }

        public IFormFile? Avatar { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
