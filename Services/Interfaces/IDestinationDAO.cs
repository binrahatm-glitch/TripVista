using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IDestinationDAO
    {
        Task<Destinations> CreateAsync(Destinations destination);
        Task<Destinations?> GetByIdAsync(Guid id);
        Task<List<Destinations>> GetByUserIdAsync(Guid userId);
        Task<List<Destinations>> GetApprovedAsync();
        Task<List<Destinations>> GetPendingAsync();
        Task<List<Destinations>> GetReviewedAsync();
        Task<bool> ApproveAsync(Guid id, Guid adminId, string? remarks);
        Task<bool> RejectAsync(Guid id, Guid adminId, string remarks);
    }
}
