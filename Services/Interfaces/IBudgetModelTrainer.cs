namespace TripVista.Services.Interfaces
{
    public interface IBudgetModelTrainer
    {
        /// Trains and saves both models only if their files don't already exist on disk.
        Task EnsureModelsTrainedAsync();

        /// Always retrains both models, preferring real package data once enough exists.
        Task<BudgetModelTrainingSummary> RetrainAsync();
    }

    public class BudgetModelTrainingSummary
    {
        public int PackagePriceTrainingRows { get; set; }
        public bool PackagePriceModelUsedRealData { get; set; }
        public int TripTierTrainingRows { get; set; }
    }
}
