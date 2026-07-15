using TripVista.Models.DTO;

namespace TripVista.Services.Interfaces
{
    public interface IBudgetEstimationService
    {
        TripBudgetResult EstimateTripBudget(TripBudgetRequest request);
        PackageBudgetEstimateResult EstimatePackagePrice(PackageBudgetEstimateRequest request);
    }
}
