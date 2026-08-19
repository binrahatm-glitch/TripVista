using Microsoft.AspNetCore.Hosting;
using Microsoft.ML;
using TripVista.Models.DTO;
using TripVista.Models.Ml;
using TripVista.Services.Interfaces;

namespace TripVista.Services.Ml
{
    public class MlBudgetEstimationService : IBudgetEstimationService
    {
        private static readonly Dictionary<string, decimal> RegionBaseDailyRate = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Northern"] = 6000m,
            ["Coastal"] = 5000m,
            ["Urban"] = 4500m,
            ["Desert"] = 4800m
        };

        private readonly IWebHostEnvironment _env;
        private readonly MLContext _mlContext = new();
        private readonly object _loadLock = new();

        private ITransformer? _packagePriceModel;
        private ITransformer? _tripTierModel;

        public MlBudgetEstimationService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public TripBudgetResult EstimateTripBudget(TripBudgetRequest request)
        {
            var dailyBudgetPerPerson = request.Budget / request.DurationDays / request.NumberOfTravelers;

            var suggestedRegion = !string.IsNullOrWhiteSpace(request.PreferredRegion) && RegionBaseDailyRate.ContainsKey(request.PreferredRegion)
                ? request.PreferredRegion
                : RegionBaseDailyRate.OrderBy(r => Math.Abs(r.Value - dailyBudgetPerPerson)).First().Key;

            var tier = PredictTier(suggestedRegion, (float)dailyBudgetPerPerson, request.DurationDays, request.NumberOfTravelers);

            var breakdown = RegionCostBaselines.Estimate(suggestedRegion, tier, request.DurationDays, request.NumberOfTravelers);

            var recommendation = tier switch
            {
                "Budget" => $"With this budget, consider hostels/guesthouses and public transport in {suggestedRegion} areas. Focus spending on food and a few key activities.",
                "Mid-range" => $"This budget supports comfortable mid-range hotels and private transport in {suggestedRegion} areas, with room for guided activities.",
                _ => $"This budget allows premium resorts, private transport, and curated experiences in {suggestedRegion} areas."
            };

            if (breakdown.TotalCost > request.Budget)
            {
                var shortfall = breakdown.TotalCost - request.Budget;
                recommendation = $"A realistic {tier.ToLowerInvariant()}-tier trip to {suggestedRegion} for {request.DurationDays} day(s) and {request.NumberOfTravelers} traveler(s) costs around PKR {breakdown.TotalCost:N0} " +
                    $"(fuel/transport, lodging, food, and activities included) - about PKR {shortfall:N0} more than your stated budget. Consider a longer-saved budget, fewer travelers, a shorter trip, or a cheaper region.";
            }

            return new TripBudgetResult
            {
                DailyBudgetPerPerson = Math.Round(dailyBudgetPerPerson, 2),
                Tier = tier,
                SuggestedRegion = suggestedRegion,
                Budget = request.Budget,
                EstimatedFuelTransportCost = Math.Round(breakdown.FuelTransportCost, 0),
                EstimatedAccommodationCost = Math.Round(breakdown.AccommodationCost, 0),
                EstimatedFoodCost = Math.Round(breakdown.FoodCost, 0),
                EstimatedActivitiesCost = Math.Round(breakdown.ActivitiesCost, 0),
                EstimatedTotalCost = Math.Round(breakdown.TotalCost, 0),
                TravelPercent = breakdown.TravelPercent,
                AccommodationPercent = breakdown.AccommodationPercent,
                FoodPercent = breakdown.FoodPercent,
                ActivitiesPercent = breakdown.ActivitiesPercent,
                Recommendation = recommendation
            };
        }

        public PackageBudgetEstimateResult EstimatePackagePrice(PackageBudgetEstimateRequest request)
        {
            var predicted = PredictPackagePrice(request.Region, request.Season, request.DurationDays, request.GroupSize, request.DestinationCount);

            return new PackageBudgetEstimateResult
            {
                MinSuggestedPrice = Math.Round((decimal)predicted * 0.9m, 0),
                MaxSuggestedPrice = Math.Round((decimal)predicted * 1.25m, 0)
            };
        }

        private float PredictPackagePrice(string region, string season, int durationDays, int groupSize, int destinationCount)
        {
            EnsureModelsLoaded();

            var engine = _mlContext.Model.CreatePredictionEngine<PackagePriceTrainingRow, PackagePricePrediction>(_packagePriceModel);
            var prediction = engine.Predict(new PackagePriceTrainingRow
            {
                Region = string.IsNullOrWhiteSpace(region) ? "Northern" : region,
                Season = string.IsNullOrWhiteSpace(season) ? "Off-Peak" : season,
                DurationDays = durationDays,
                GroupSize = groupSize,
                DestinationCount = Math.Max(destinationCount, 1)
            });

            return Math.Max(prediction.PricePerPerson, 0);
        }

        private string PredictTier(string region, float dailyBudgetPerPerson, int durationDays, int numberOfTravelers)
        {
            EnsureModelsLoaded();

            var engine = _mlContext.Model.CreatePredictionEngine<TripTierTrainingRow, TripTierPrediction>(_tripTierModel);
            var prediction = engine.Predict(new TripTierTrainingRow
            {
                Region = region,
                DailyBudgetPerPerson = dailyBudgetPerPerson,
                DurationDays = durationDays,
                NumberOfTravelers = numberOfTravelers
            });

            return prediction.Tier;
        }

        private void EnsureModelsLoaded()
        {
            if (_packagePriceModel != null && _tripTierModel != null) return;

            lock (_loadLock)
            {
                _packagePriceModel ??= _mlContext.Model.Load(MlModelPaths.PackagePriceModelPath(_env), out _);
                _tripTierModel ??= _mlContext.Model.Load(MlModelPaths.TripTierModelPath(_env), out _);
            }
        }
    }
}
