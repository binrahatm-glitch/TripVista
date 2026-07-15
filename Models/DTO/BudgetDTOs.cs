using System.ComponentModel.DataAnnotations;

namespace TripVista.Models.DTO
{
    public class TripBudgetRequest
    {
        [Range(1, double.MaxValue)]
        public decimal Budget { get; set; }

        [Range(1, 365)]
        public int DurationDays { get; set; }

        [Range(1, 100)]
        public int NumberOfTravelers { get; set; }

        public string? PreferredRegion { get; set; }
    }

    public class TripBudgetResult
    {
        public decimal DailyBudgetPerPerson { get; set; }
        public string Tier { get; set; } = string.Empty;
        public string SuggestedRegion { get; set; } = string.Empty;
        public int TravelPercent { get; set; }
        public int AccommodationPercent { get; set; }
        public int FoodPercent { get; set; }
        public int ActivitiesPercent { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    public class PackageBudgetEstimateRequest
    {
        [Range(1, 365)]
        public int DurationDays { get; set; }

        public string Region { get; set; } = "Northern";

        [Range(1, 500)]
        public int GroupSize { get; set; }

        public string Season { get; set; } = "Peak";
    }

    public class PackageBudgetEstimateResult
    {
        public decimal MinSuggestedPrice { get; set; }
        public decimal MaxSuggestedPrice { get; set; }
    }
}
