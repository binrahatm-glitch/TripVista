namespace TripVista.Services.Ml
{
    /// <summary>
    /// Reference real-world cost floors used to turn a predicted tier into an itemized,
    /// region- and duration-aware cost breakdown (fuel/transport is mostly a fixed cost per
    /// trip, not per day, which is why short trips to remote regions skew heavily toward it).
    /// These are heuristic PKR estimates, not learned figures - they exist so the breakdown
    /// reflects real minimums (e.g. northern-area lodging isn't rentable for PKR 700) instead
    /// of applying a flat percentage split to whatever budget was entered.
    /// </summary>
    public static class RegionCostBaselines
    {
        private class Profile
        {
            public decimal FuelTransportPerTrip; // round-trip, per traveler, mid-range tier
            public decimal AccommodationPerNight; // per room, mid-range tier
            public decimal FoodPerDay; // per traveler, mid-range tier
            public decimal ActivitiesPerDay; // per traveler, mid-range tier
        }

        private static readonly Dictionary<string, Profile> Profiles = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Northern"] = new Profile { FuelTransportPerTrip = 12000m, AccommodationPerNight = 4000m, FoodPerDay = 2500m, ActivitiesPerDay = 1500m },
            ["Coastal"] = new Profile { FuelTransportPerTrip = 6000m, AccommodationPerNight = 3200m, FoodPerDay = 2200m, ActivitiesPerDay = 1200m },
            ["Urban"] = new Profile { FuelTransportPerTrip = 2500m, AccommodationPerNight = 2800m, FoodPerDay = 2000m, ActivitiesPerDay = 1000m },
            ["Desert"] = new Profile { FuelTransportPerTrip = 8000m, AccommodationPerNight = 3200m, FoodPerDay = 2000m, ActivitiesPerDay = 1200m }
        };

        // Applied to Accommodation/Food/Activities - these scale a lot with comfort level.
        private static readonly Dictionary<string, decimal> StayTierMultiplier = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Budget"] = 0.55m,
            ["Mid-range"] = 1.0m,
            ["Premium"] = 1.9m
        };

        // Applied to Fuel/Transport - less elastic than lodging/food, so a narrower spread.
        private static readonly Dictionary<string, decimal> TransportTierMultiplier = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Budget"] = 0.8m,
            ["Mid-range"] = 1.0m,
            ["Premium"] = 1.3m
        };

        public const int RoomCapacity = 2; // assumed travelers sharing a room

        public class CostBreakdown
        {
            public decimal FuelTransportCost { get; set; }
            public decimal AccommodationCost { get; set; }
            public decimal FoodCost { get; set; }
            public decimal ActivitiesCost { get; set; }
            public decimal TotalCost => FuelTransportCost + AccommodationCost + FoodCost + ActivitiesCost;

            public int TravelPercent => TotalCost <= 0 ? 0 : (int)Math.Round(FuelTransportCost / TotalCost * 100);
            public int AccommodationPercent => TotalCost <= 0 ? 0 : (int)Math.Round(AccommodationCost / TotalCost * 100);
            public int FoodPercent => TotalCost <= 0 ? 0 : (int)Math.Round(FoodCost / TotalCost * 100);
            public int ActivitiesPercent => TotalCost <= 0 ? 0 : Math.Max(0, 100 - TravelPercent - AccommodationPercent - FoodPercent);
        }

        public static CostBreakdown Estimate(string region, string tier, int durationDays, int travelers)
        {
            var profile = Profiles.TryGetValue(region, out var p) ? p : Profiles["Northern"];
            var stayMultiplier = StayTierMultiplier.TryGetValue(tier, out var sm) ? sm : 1.0m;
            var transportMultiplier = TransportTierMultiplier.TryGetValue(tier, out var tm) ? tm : 1.0m;

            var nights = Math.Max(durationDays - 1, 0);
            var rooms = (int)Math.Ceiling(travelers / (decimal)RoomCapacity);

            return new CostBreakdown
            {
                FuelTransportCost = profile.FuelTransportPerTrip * transportMultiplier * travelers,
                AccommodationCost = nights == 0 ? 0 : profile.AccommodationPerNight * stayMultiplier * nights * rooms,
                FoodCost = profile.FoodPerDay * stayMultiplier * durationDays * travelers,
                ActivitiesCost = profile.ActivitiesPerDay * stayMultiplier * durationDays * travelers
            };
        }
    }
}
