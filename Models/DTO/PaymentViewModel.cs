using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class PaymentViewModel
    {
        public Guid BookingId { get; set; }
        public string PackageTitle { get; set; } = string.Empty;
        public int TravelersCount { get; set; }
        public decimal Amount { get; set; }

        [Required, MaxLength(100)]
        public string CardholderName { get; set; } = string.Empty;

        [Required, CreditCard]
        public string CardNumber { get; set; } = string.Empty;

        [Required, RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Use MM/YY format.")]
        public string ExpiryDate { get; set; } = string.Empty;

        [Required, RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string Cvv { get; set; } = string.Empty;
    }
}
