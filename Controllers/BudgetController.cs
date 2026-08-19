using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Models.DTO;
using TripVista.Services.Interfaces;
using TripVista.Services.Ml;

namespace TripVista.Controllers
{
    public class BudgetController : Controller
    {
        private readonly IBudgetEstimationService _budgetService;
        private readonly IPackageDAO _packageDAO;

        public BudgetController(IBudgetEstimationService budgetService, IPackageDAO packageDAO)
        {
            _budgetService = budgetService;
            _packageDAO = packageDAO;
        }

        [HttpGet]
        [RequireRole("user")]
        public IActionResult PlanTrip()
        {
            return View(new TripBudgetRequest { DurationDays = 5, NumberOfTravelers = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("user")]
        public async Task<IActionResult> PlanTrip(TripBudgetRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = _budgetService.EstimateTripBudget(request);
            result.AffordablePackages = await FindAffordablePackagesAsync(request, result);

            ViewBag.Result = result;
            return View(request);
        }

        private async Task<List<AffordablePackageSuggestion>> FindAffordablePackagesAsync(TripBudgetRequest request, TripBudgetResult result)
        {
            var packages = await _packageDAO.GetActiveAsync();

            var withCost = packages.Select(p =>
            {
                var total = p.PricePerPerson * request.NumberOfTravelers;

                // Split the package's real, bookable total using a realistic per-category ratio for its
                // own region/duration (rather than reusing the trip request's percentages, which were
                // computed for a possibly different duration and would misrepresent this package's costs).
                var ratio = RegionCostBaselines.Estimate(p.Region ?? result.SuggestedRegion, result.Tier, p.DurationDays, request.NumberOfTravelers);

                return new AffordablePackageSuggestion
                {
                    PackageId = p.PackageId,
                    Title = p.Title,
                    Region = p.Region,
                    DurationDays = p.DurationDays,
                    TotalEstimatedCost = Math.Round(total, 0),
                    FuelTransportCost = Math.Round(total * ratio.TravelPercent / 100m, 0),
                    AccommodationCost = Math.Round(total * ratio.AccommodationPercent / 100m, 0),
                    FoodCost = Math.Round(total * ratio.FoodPercent / 100m, 0),
                    ActivitiesCost = Math.Round(total * ratio.ActivitiesPercent / 100m, 0),
                    OverBudgetBy = Math.Max(total - request.Budget, 0)
                };
            }).ToList();

            var withinBudget = withCost.Where(p => p.OverBudgetBy <= 0).OrderByDescending(p => p.TotalEstimatedCost).Take(5).ToList();
            if (withinBudget.Any())
                return withinBudget;

            // Nothing fits the stated budget - show the closest options anyway, clearly flagged as
            // over budget, instead of leaving the user with no suggestions at all.
            return withCost.OrderBy(p => p.OverBudgetBy).Take(3).ToList();
        }

        [HttpGet]
        [RequireRole("admin")]
        public IActionResult EstimatePackagePrice(int durationDays, string region, int groupSize, string season, int destinationCount)
        {
            var result = _budgetService.EstimatePackagePrice(new PackageBudgetEstimateRequest
            {
                DurationDays = durationDays <= 0 ? 1 : durationDays,
                Region = region,
                GroupSize = groupSize <= 0 ? 1 : groupSize,
                Season = season,
                DestinationCount = destinationCount <= 0 ? 1 : destinationCount
            });

            return Json(result);
        }
    }
}
