using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IBookingDAO
    {
        Task<List<Bookings>> GetByUserIdAsync(Guid userId);
        Task<Bookings?> GetByIdAsync(Guid id);
        Task<Bookings> CreateConfirmedBookingAsync(Guid userId, Guid packageId, int numberOfTravelers, decimal totalPrice);
        Task<bool> AddTravelerDetailsAsync(Guid bookingId, List<BookingTravelers> travelers);
        Task<(bool success, string? error)> CancelAsync(Guid bookingId, Guid requestingUserId);
    }
}
