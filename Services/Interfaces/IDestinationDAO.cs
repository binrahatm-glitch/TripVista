using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IDestinationDAO
    {
        Task<Destinations> CreateAsync(Destinations destination);
        Task<bool> UpdateAsync(Destinations destination);
        Task<bool> IsLinkedToPackageAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<Destinations?> GetByIdAsync(Guid id);
        Task<List<Destinations>> GetByUserIdAsync(Guid userId);
        Task<List<Destinations>> GetApprovedAsync();
        Task<List<Destinations>> GetPendingAsync();
        Task<List<Destinations>> GetReviewedAsync();
        Task<(bool success, string? error)> ApproveAsync(Guid id, Guid adminId, string? remarks);
        Task<bool> RejectAsync(Guid id, Guid adminId, string remarks);
        Task<List<Destinations>> SearchAsyncByName(string name, Guid? excludeId = null);
    }
}
