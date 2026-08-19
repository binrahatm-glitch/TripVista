using Microsoft.ML.Data;

namespace TripVista.Models.Ml
{
    public class TripTierTrainingRow
    {
        public string Region { get; set; } = string.Empty;
        public float DailyBudgetPerPerson { get; set; }
        public float DurationDays { get; set; }
        public float NumberOfTravelers { get; set; }
        public string Tier { get; set; } = string.Empty;
    }

    public class TripTierPrediction
    {
        [ColumnName("PredictedLabel")]
        public string Tier { get; set; } = string.Empty;
    }
}
