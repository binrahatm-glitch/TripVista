using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Models.DTO;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class BudgetController : Controller
    {
        private readonly IBudgetEstimationService _budgetService;

        public BudgetController(IBudgetEstimationService budgetService)
        {
            _budgetService = budgetService;
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
        public IActionResult PlanTrip(TripBudgetRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var result = _budgetService.EstimateTripBudget(request);
            ViewBag.Result = result;
            return View(request);
        }

        [HttpGet]
        [RequireRole("admin")]
        public IActionResult EstimatePackagePrice(int durationDays, string region, int groupSize, string season)
        {
            var result = _budgetService.EstimatePackagePrice(new PackageBudgetEstimateRequest
            {
                DurationDays = durationDays <= 0 ? 1 : durationDays,
                Region = region,
                GroupSize = groupSize <= 0 ? 1 : groupSize,
                Season = season
            });

            return Json(result);
        }
    }
}
