using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using TripVista.Data;
using TripVista.Models.Ml;
using TripVista.Services.Interfaces;

namespace TripVista.Services.Ml
{
    public class BudgetModelTrainer : IBudgetModelTrainer
    {
        // Below this many real packages, historical data is too thin to train on - fall back to synthetic data.
        private const int MinRealRowsForPackagePriceModel = 20;

        private static readonly Dictionary<string, float> RegionBaseDailyRate = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Northern"] = 6000f,
            ["Coastal"] = 5000f,
            ["Urban"] = 4500f,
            ["Desert"] = 4800f
        };

        private readonly SmartTripContext _context;
        private readonly IWebHostEnvironment _env;

        public BudgetModelTrainer(SmartTripContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public Task EnsureModelsTrainedAsync()
        {
            var packagePricePath = MlModelPaths.PackagePriceModelPath(_env);
            var tripTierPath = MlModelPaths.TripTierModelPath(_env);

            if (File.Exists(packagePricePath) && File.Exists(tripTierPath))
                return Task.CompletedTask;

            return RetrainAsync();
        }

        public async Task<BudgetModelTrainingSummary> RetrainAsync()
        {
            var mlContext = new MLContext(seed: 42);
            var rng = new Random(42);

            var realPackageData = await TryGetRealPackagePriceDataAsync();
            var packagePriceRows = realPackageData ?? GenerateSyntheticPackagePriceData(800, rng);
            TrainPackagePriceModel(mlContext, packagePriceRows);

            var tripTierRows = GenerateSyntheticTripTierData(1200, rng);
            TrainTripTierModel(mlContext, tripTierRows);

            return new BudgetModelTrainingSummary
            {
                PackagePriceTrainingRows = packagePriceRows.Count,
                PackagePriceModelUsedRealData = realPackageData != null,
                TripTierTrainingRows = tripTierRows.Count
            };
        }

        private async Task<List<PackagePriceTrainingRow>?> TryGetRealPackagePriceDataAsync()
        {
            var packages = await _context.Packages
                .Include(p => p.PackageDestinations)
                .Where(p => p.Region != null && p.PricePerPerson > 0 && p.DurationDays > 0 && p.MaxGroupSize > 0)
                .ToListAsync();

            if (packages.Count < MinRealRowsForPackagePriceModel) return null;

            return packages.Select(p => new PackagePriceTrainingRow
            {
                Region = p.Region!,
                Season = p.TourStartDate.Month is >= 5 and <= 8 or 12 ? "Peak" : "Off-Peak",
                DurationDays = p.DurationDays,
                GroupSize = p.MaxGroupSize,
                DestinationCount = Math.Max(p.PackageDestinations?.Count ?? 0, 1),
                PricePerPerson = (float)p.PricePerPerson
            }).ToList();
        }

        private static List<PackagePriceTrainingRow> GenerateSyntheticPackagePriceData(int count, Random rng)
        {
            var regions = RegionBaseDailyRate.Keys.ToArray();
            var seasons = new[] { "Peak", "Off-Peak" };
            var rows = new List<PackagePriceTrainingRow>(count);

            for (int i = 0; i < count; i++)
            {
                var region = regions[rng.Next(regions.Length)];
                var season = seasons[rng.Next(seasons.Length)];
                var duration = rng.Next(1, 15);
                var groupSize = rng.Next(1, 25);
                var destinationCount = rng.Next(1, 7);

                var baseRate = RegionBaseDailyRate[region];
                var seasonMultiplier = season == "Peak" ? 1.15f : 0.9f;
                var groupDiscount = groupSize >= 15 ? 0.85f : groupSize >= 8 ? 0.92f : 1.0f;
                // Each extra stop beyond the first adds inter-destination transport/logistics cost.
                var destinationMultiplier = 1f + (destinationCount - 1) * 0.06f;

                var basePrice = baseRate * duration * seasonMultiplier * groupDiscount * destinationMultiplier;
                var noise = 0.85f + (float)rng.NextDouble() * 0.3f; // +/- 15% real-world variance

                rows.Add(new PackagePriceTrainingRow
                {
                    Region = region,
                    Season = season,
                    DurationDays = duration,
                    GroupSize = groupSize,
                    DestinationCount = destinationCount,
                    PricePerPerson = basePrice * noise
                });
            }

            return rows;
        }

        private static List<TripTierTrainingRow> GenerateSyntheticTripTierData(int count, Random rng)
        {
            var regions = RegionBaseDailyRate.Keys.ToArray();
            var tiers = new[] { "Budget", "Mid-range", "Premium" };
            var rows = new List<TripTierTrainingRow>(count);

            for (int i = 0; i < count; i++)
            {
                var region = regions[rng.Next(regions.Length)];
                var duration = rng.Next(1, 21);
                var travelers = rng.Next(1, 15);
                var dailyBudget = (float)(rng.NextDouble() * 15000);

                var tier = dailyBudget switch
                {
                    < 3000 => "Budget",
                    < 7000 => "Mid-range",
                    _ => "Premium"
                };

                // 5% label noise near the rule boundaries so the classifier learns a real decision
                // boundary instead of just memorizing the threshold it was bootstrapped from.
                if (rng.NextDouble() < 0.05)
                {
                    tier = tiers[rng.Next(tiers.Length)];
                }

                rows.Add(new TripTierTrainingRow
                {
                    Region = region,
                    DailyBudgetPerPerson = dailyBudget,
                    DurationDays = duration,
                    NumberOfTravelers = travelers,
                    Tier = tier
                });
            }

            return rows;
        }

        private void TrainPackagePriceModel(MLContext mlContext, List<PackagePriceTrainingRow> rows)
        {
            var data = mlContext.Data.LoadFromEnumerable(rows);

            var pipeline = mlContext.Transforms.Categorical.OneHotEncoding("RegionEncoded", nameof(PackagePriceTrainingRow.Region))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("SeasonEncoded", nameof(PackagePriceTrainingRow.Season)))
                .Append(mlContext.Transforms.Concatenate("Features", "RegionEncoded", "SeasonEncoded",
                    nameof(PackagePriceTrainingRow.DurationDays), nameof(PackagePriceTrainingRow.GroupSize), nameof(PackagePriceTrainingRow.DestinationCount)))
                .Append(mlContext.Regression.Trainers.FastTree(
                    labelColumnName: nameof(PackagePriceTrainingRow.PricePerPerson),
                    featureColumnName: "Features"));

            var model = pipeline.Fit(data);

            Directory.CreateDirectory(MlModelPaths.ModelsDirectory(_env));
            mlContext.Model.Save(model, data.Schema, MlModelPaths.PackagePriceModelPath(_env));
        }

        private void TrainTripTierModel(MLContext mlContext, List<TripTierTrainingRow> rows)
        {
            var data = mlContext.Data.LoadFromEnumerable(rows);

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TripTierTrainingRow.Tier))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("RegionEncoded", nameof(TripTierTrainingRow.Region)))
                .Append(mlContext.Transforms.Concatenate("Features", "RegionEncoded",
                    nameof(TripTierTrainingRow.DailyBudgetPerPerson), nameof(TripTierTrainingRow.DurationDays), nameof(TripTierTrainingRow.NumberOfTravelers)))
                .Append(mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(labelColumnName: "Label", featureColumnName: "Features"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            var model = pipeline.Fit(data);

            Directory.CreateDirectory(MlModelPaths.ModelsDirectory(_env));
            mlContext.Model.Save(model, data.Schema, MlModelPaths.TripTierModelPath(_env));
        }
    }
}
