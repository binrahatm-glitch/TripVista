using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IPackageDAO
    {
        Task<List<Packages>> GetAllAsync();
        Task<List<Packages>> GetActiveAsync();
        Task<Packages?> GetByIdAsync(Guid id);
        Task CreateAsync(Packages pkg, List<Guid> destinationIds);
        Task<bool> UpdateAsync(Packages pkg, List<Guid> destinationIds);
        Task<bool> ExistsByTitleAsync(string title, Guid? excludeId);
        Task<bool> DeactivateAsync(Guid id);
        Task<bool> HasBookingsAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
    }
}
