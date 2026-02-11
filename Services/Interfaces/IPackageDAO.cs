using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IPackageDAO
    {
        IEnumerable<Packages> GetAll();
        Task<List<Packages>> GetAllPackagesWithDestinationsAsync();
        Packages? GetById(Guid id);
        void Create(Packages pkg);
        void Update(Packages pkg);
        void Delete(Guid id);
        void Save();
    }
}
