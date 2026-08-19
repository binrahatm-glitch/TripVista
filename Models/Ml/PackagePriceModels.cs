using Microsoft.ML.Data;

namespace TripVista.Models.Ml
{
    public class PackagePriceTrainingRow
    {
        public string Region { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public float DurationDays { get; set; }
        public float GroupSize { get; set; }
        public float DestinationCount { get; set; }
        public float PricePerPerson { get; set; }
    }

    public class PackagePricePrediction
    {
        [ColumnName("Score")]
        public float PricePerPerson { get; set; }
    }
}
