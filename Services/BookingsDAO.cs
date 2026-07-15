using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class BookingsDAO : IBookingDAO
    {
        private static readonly TimeSpan CancellationWindow = TimeSpan.FromDays(3);

        private readonly SmartTripContext _context;

        public BookingsDAO(SmartTripContext context)
        {
            _context = context;
        }

        public async Task<List<Bookings>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Bookings
                .Include(b => b.Package)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Bookings?> GetByIdAsync(Guid id)
        {
            return await _context.Bookings
                .Include(b => b.Package)
                .Include(b => b.BookingTravelers)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<Bookings> CreateConfirmedBookingAsync(Guid userId, Guid packageId, int numberOfTravelers, decimal totalPrice)
        {
            var booking = new Bookings
            {
                BookingId = Guid.NewGuid(),
                UserId = userId,
                PackageId = packageId,
                TravelersCount = numberOfTravelers,
                TotalPrice = totalPrice,
                Status = "Confirmed",
                AreTravelerDetailsSubmitted = false,
                BookingDate = DateTime.Now
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> AddTravelerDetailsAsync(Guid bookingId, List<BookingTravelers> travelers)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return false;

            foreach (var traveler in travelers)
            {
                traveler.Id = Guid.NewGuid();
                traveler.BookingId = bookingId;
            }

            await _context.BookingTravelers.AddRangeAsync(travelers);
            booking.AreTravelerDetailsSubmitted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool success, string? error)> CancelAsync(Guid bookingId, Guid requestingUserId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Package)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null || booking.UserId != requestingUserId)
                return (false, "Booking not found.");

            if (booking.Status != "Confirmed")
                return (false, "Only confirmed bookings can be cancelled.");

            if (booking.Package == null || booking.Package.TourStartDate - DateTime.Now < CancellationWindow)
                return (false, "Bookings can only be cancelled at least 3 days before the tour start date.");

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
