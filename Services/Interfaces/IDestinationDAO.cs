using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IDestinationDAO
    {
        List<Destinations> GetAll();
        Destinations? GetById(Guid id);
        void Create(Destinations dest);
        void Update(Destinations dest);
        void Delete(Guid id);
        void Save();
    }
}
