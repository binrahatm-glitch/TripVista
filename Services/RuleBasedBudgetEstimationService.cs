using TripVista.Models.DTO;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class RuleBasedBudgetEstimationService : IBudgetEstimationService
    {
        // Reference base daily cost per person (PKR) by region, at "Mid-range" tier.
        private static readonly Dictionary<string, decimal> RegionBaseDailyRate = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Northern"] = 6000m,
            ["Coastal"] = 5000m,
            ["Urban"] = 4500m,
            ["Desert"] = 4800m
        };

        public TripBudgetResult EstimateTripBudget(TripBudgetRequest request)
        {
            var dailyBudgetPerPerson = request.Budget / request.DurationDays / request.NumberOfTravelers;

            string tier = dailyBudgetPerPerson switch
            {
                < 3000 => "Budget",
                < 7000 => "Mid-range",
                _ => "Premium"
            };

            var suggestedRegion = !string.IsNullOrWhiteSpace(request.PreferredRegion) && RegionBaseDailyRate.ContainsKey(request.PreferredRegion)
                ? request.PreferredRegion
                : RegionBaseDailyRate
                    .OrderBy(r => Math.Abs(r.Value - dailyBudgetPerPerson))
                    .First().Key;

            var recommendation = tier switch
            {
                "Budget" => $"With this budget, consider hostels/guesthouses and public transport in {suggestedRegion} areas. Focus spending on food and a few key activities.",
                "Mid-range" => $"This budget supports comfortable mid-range hotels and private transport in {suggestedRegion} areas, with room for guided activities.",
                _ => $"This budget allows premium resorts, private transport, and curated experiences in {suggestedRegion} areas."
            };

            return new TripBudgetResult
            {
                DailyBudgetPerPerson = Math.Round(dailyBudgetPerPerson, 2),
                Tier = tier,
                SuggestedRegion = suggestedRegion,
                TravelPercent = 20,
                AccommodationPercent = 35,
                FoodPercent = 25,
                ActivitiesPercent = 20,
                Recommendation = recommendation
            };
        }

        public PackageBudgetEstimateResult EstimatePackagePrice(PackageBudgetEstimateRequest request)
        {
            var baseRate = RegionBaseDailyRate.TryGetValue(request.Region, out var rate) ? rate : RegionBaseDailyRate["Northern"];

            var seasonMultiplier = string.Equals(request.Season, "Peak", StringComparison.OrdinalIgnoreCase) ? 1.15m : 0.9m;

            // Larger groups get a small per-person discount on shared costs (transport/guides).
            var groupDiscount = request.GroupSize switch
            {
                >= 15 => 0.85m,
                >= 8 => 0.92m,
                _ => 1.0m
            };

            var basePricePerPerson = baseRate * request.DurationDays * seasonMultiplier * groupDiscount;

            return new PackageBudgetEstimateResult
            {
                MinSuggestedPrice = Math.Round(basePricePerPerson * 0.9m, 0),
                MaxSuggestedPrice = Math.Round(basePricePerPerson * 1.25m, 0)
            };
        }
    }
}
