using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class TravelerDetailInput
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Range(0, 120)]
        public int Age { get; set; }

        [Required, MaxLength(50)]
        public string CNICOrPassportNumber { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class AddTravelerDetailsViewModel
    {
        public Guid BookingId { get; set; }
        public string PackageTitle { get; set; } = string.Empty;
        public List<TravelerDetailInput> Travelers { get; set; } = new();
    }
}
