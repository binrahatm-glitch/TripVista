using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class TravelerDetailInput
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Age must be greater than 0.")]
        public int Age { get; set; }

        [Required, MaxLength(50)]
        public string CNICOrPassportNumber { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class AddTravelerDetailsViewModel : IValidatableObject
    {
        public Guid BookingId { get; set; }
        public string PackageTitle { get; set; } = string.Empty;
        public List<TravelerDetailInput> Travelers { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < Travelers.Count; i++)
            {
                var cnic = Travelers[i].CNICOrPassportNumber?.Trim();
                if (!string.IsNullOrEmpty(cnic) && !seen.Add(cnic))
                {
                    yield return new ValidationResult(
                        "Each traveler must have a unique CNIC/Passport number.",
                        new[] { $"{nameof(Travelers)}[{i}].{nameof(TravelerDetailInput.CNICOrPassportNumber)}" });
                }
            }
        }
    }
}
