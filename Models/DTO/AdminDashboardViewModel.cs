namespace TripVista.Models.DTO
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int ActivePackagesCount { get; set; }
        public int TotalUsersCount { get; set; }
        public int PendingDestinationsCount { get; set; }

        public List<BookingStatusCount> BookingStatusCounts { get; set; } = new();
        public List<MonthlyRevenue> RevenueByMonth { get; set; } = new();
        public List<PackageBookingCount> TopPackages { get; set; } = new();
    }

    public class BookingStatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MonthlyRevenue
    {
        public string MonthLabel { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class PackageBookingCount
    {
        public string Title { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
