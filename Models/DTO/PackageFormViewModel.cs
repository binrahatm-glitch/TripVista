using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class PackageFormViewModel
    {
        public Guid? PackageId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? Details { get; set; }

        public string? ImagePath { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price per person must be greater than 0.")]
        public decimal PricePerPerson { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Duration must be at least 1 day.")]
        public int DurationDays { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max group size must be at least 1.")]
        public int MaxGroupSize { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime TourStartDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime TourEndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public List<PackageDestinationOption> DestinationOptions { get; set; } = new();
    }

    public class PackageDestinationOption
    {
        public Guid DestinationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}
