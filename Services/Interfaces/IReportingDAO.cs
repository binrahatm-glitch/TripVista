using TripVista.Models.DTO;

namespace TripVista.Services.Interfaces
{
    public interface IReportingDAO
    {
        Task<AdminDashboardViewModel> GetDashboardStatsAsync();
    }
}
