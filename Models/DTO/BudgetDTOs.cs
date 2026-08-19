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

        // Realistic itemized cost for this trip (region/tier/duration/traveler-aware), independent of
        // whether the user's stated Budget actually covers it.
        public decimal EstimatedFuelTransportCost { get; set; }
        public decimal EstimatedAccommodationCost { get; set; }
        public decimal EstimatedFoodCost { get; set; }
        public decimal EstimatedActivitiesCost { get; set; }
        public decimal EstimatedTotalCost { get; set; }
        public int TravelPercent { get; set; }
        public int AccommodationPercent { get; set; }
        public int FoodPercent { get; set; }
        public int ActivitiesPercent { get; set; }

        public decimal Budget { get; set; }
        public decimal BudgetShortfall => Math.Max(EstimatedTotalCost - Budget, 0);
        public bool IsBudgetRealistic => BudgetShortfall <= 0;

        public string Recommendation { get; set; } = string.Empty;

        public List<AffordablePackageSuggestion> AffordablePackages { get; set; } = new();
    }

    public class AffordablePackageSuggestion
    {
        public Guid PackageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Region { get; set; }
        public int DurationDays { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public decimal FuelTransportCost { get; set; }
        public decimal AccommodationCost { get; set; }
        public decimal FoodCost { get; set; }
        public decimal ActivitiesCost { get; set; }
        public decimal OverBudgetBy { get; set; }
    }

    public class PackageBudgetEstimateRequest
    {
        [Range(1, 365)]
        public int DurationDays { get; set; }

        public string Region { get; set; } = "Northern";

        [Range(1, 500)]
        public int GroupSize { get; set; }

        public string Season { get; set; } = "Peak";

        [Range(1, 50)]
        public int DestinationCount { get; set; } = 1;
    }

    public class PackageBudgetEstimateResult
    {
        public decimal MinSuggestedPrice { get; set; }
        public decimal MaxSuggestedPrice { get; set; }
    }
}
