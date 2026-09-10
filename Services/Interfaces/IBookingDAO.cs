using TripVista.Models.Entities;

namespace TripVista.Services.Interfaces
{
    public interface IBookingDAO
    {
        Task<List<Bookings>> GetBookings();
        Task<List<Bookings>> GetByUserIdAsync(Guid userId);
        Task<Bookings?> GetByIdAsync(Guid id);
        Task<Bookings> CreatePendingBookingAsync(Guid userId, Guid packageId, int numberOfTravelers, decimal totalPrice);
        Task<bool> HasActiveBookingAsync(Guid userId, Guid packageId);
        Task<(bool success, string? error)> CompletePaymentAsync(Guid bookingId, Guid userId, string paymentReference);
        Task<bool> AddTravelerDetailsAsync(Guid bookingId, List<BookingTravelers> travelers);
        Task<(bool success, string? error)> CancelAsync(Guid bookingId, Guid requestingUserId);
        Task<(bool success, string? error)> AdminUpdateTravelersAndPriceAsync(Guid bookingId, int travelersCount, decimal? totalPrice);
        Task<(bool success, string? error)> AdminAddTravelersAsync(Guid bookingId, List<BookingTravelers> newTravelers, int newCount, decimal? newPrice);
        Task<(bool success, string? error, int remainingCount)> RemoveTravelerAsync(Guid bookingId, Guid travelerId, int newCount, decimal? newPrice);
        Task<(bool success, string? error)> AdminConfirmAsync(Guid bookingId);
        Task<(bool success, string? error)> AdminCompleteAsync(Guid bookingId);
        Task<(bool success, string? error)> AdminCancelAsync(Guid bookingId);
        Task<(bool success, string? error)> AdminMarkPaidAsync(Guid bookingId);
    }
}
