using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IBookingDAO
    {
        IEnumerable<Bookings> GetAll();
        Bookings? GetById(Guid id);
        IEnumerable<Bookings> GetByUserId(Guid userId);
        void Create(Bookings booking);
        void Update(Bookings booking);
        void Delete(Guid id);
        void Save();
    }
}
