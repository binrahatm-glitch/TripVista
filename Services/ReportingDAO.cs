using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Helpers;
using TripVista.Models.DTO;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class ReportingDAO : IReportingDAO
    {
        private static readonly string[] StatusOrder = { "PendingPayment", "Confirmed", "Completed", "Cancelled" };

        private readonly SmartTripContext _context;

        public ReportingDAO(SmartTripContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            var bookings = await _context.Bookings.Include(b => b.Package).ToListAsync();
            var activePackagesCount = await _context.Packages.CountAsync(p => p.IsActive);
            var totalUsersCount = await _context.Users.CountAsync(u => u.Role == "user");
            var pendingDestinationsCount = await _context.Destinations.CountAsync(d => d.Status == "Pending");

            var vm = new AdminDashboardViewModel
            {
                TotalRevenue = bookings.Where(b => b.PaymentStatus == "Paid").Sum(b => b.TotalPrice ?? 0),
                TotalBookings = bookings.Count,
                ActivePackagesCount = activePackagesCount,
                TotalUsersCount = totalUsersCount,
                PendingDestinationsCount = pendingDestinationsCount,

                BookingStatusCounts = StatusOrder
                    .Select(status => new BookingStatusCount
                    {
                        Status = status,
                        Count = bookings.Count(b => b.Status == status)
                    })
                    .ToList()
            };

            var months = Enumerable.Range(0, 6)
                .Select(offset => DateTimeExtensions.PakistanNow().AddMonths(-5 + offset))
                .Select(d => new DateTime(d.Year, d.Month, 1));

            vm.RevenueByMonth = months
                .Select(month => new MonthlyRevenue
                {
                    MonthLabel = month.ToString("MMM yy"),
                    Revenue = bookings
                        .Where(b => b.PaymentStatus == "Paid" && b.PaidAt.HasValue
                                    && b.PaidAt.Value.Year == month.Year && b.PaidAt.Value.Month == month.Month)
                        .Sum(b => b.TotalPrice ?? 0)
                })
                .ToList();

            vm.TopPackages = bookings
                .Where(b => b.Status != "Cancelled" && b.Package != null)
                .GroupBy(b => b.Package!.Title)
                .Select(g => new PackageBookingCount { Title = g.Key, Count = g.Count() })
                .OrderByDescending(p => p.Count)
                .Take(5)
                .ToList();

            return vm;
        }
    }
}
