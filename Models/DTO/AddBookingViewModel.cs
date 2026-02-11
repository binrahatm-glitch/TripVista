namespace TripVista.Models.DTO
{
    public class AddBookingViewModel
    {
        public Guid PackageId { get; set; }
        public Guid UserId { get; set; }
        public string PackageTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Discount { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public int NumberOfPersons { get; set; }
        public DateTime TravelDate { get; set; }
    }
}
