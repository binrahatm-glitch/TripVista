using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class ExistingTravelerInfo
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string CNICOrPassportNumber { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
    }

    public class ManageBookingTravelersViewModel : IValidatableObject
    {
        public Guid BookingId { get; set; }
        public string PackageTitle { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Traveler count must be at least 1.")]
        public int NewCount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
        public decimal? NewPrice { get; set; }

        public List<ExistingTravelerInfo> ExistingTravelers { get; set; } = new();
        public List<TravelerDetailInput> AdditionalTravelers { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var seen = new HashSet<string>(
                ExistingTravelers
                    .Select(t => t.CNICOrPassportNumber?.Trim())
                    .Where(cnic => !string.IsNullOrEmpty(cnic))!,
                StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < AdditionalTravelers.Count; i++)
            {
                var cnic = AdditionalTravelers[i].CNICOrPassportNumber?.Trim();
                if (!string.IsNullOrEmpty(cnic) && !seen.Add(cnic))
                {
                    yield return new ValidationResult(
                        "Each traveler must have a unique CNIC/Passport number.",
                        new[] { $"{nameof(AdditionalTravelers)}[{i}].{nameof(TravelerDetailInput.CNICOrPassportNumber)}" });
                }
            }
        }
    }
}
