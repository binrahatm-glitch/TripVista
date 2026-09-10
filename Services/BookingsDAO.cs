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


        public async Task<List<Bookings>> GetBookings()
        {
            return await _context.Bookings
               .Include(b => b.User)
               .Include(b => b.Package)
               .OrderByDescending(b => b.BookingDate)
               .ToListAsync();
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
                .Include(b => b.User)
                .Include(b => b.Package)
                .Include(b => b.BookingTravelers)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task<Bookings> CreatePendingBookingAsync(Guid userId, Guid packageId, int numberOfTravelers, decimal totalPrice)
        {
            var booking = new Bookings
            {
                BookingId = Guid.NewGuid(),
                UserId = userId,
                PackageId = packageId,
                TravelersCount = numberOfTravelers,
                TotalPrice = totalPrice,
                Status = "PendingPayment",
                PaymentStatus = "Pending",
                AreTravelerDetailsSubmitted = false,
                BookingDate = DateTime.Now
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> HasActiveBookingAsync(Guid userId, Guid packageId)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.UserId == userId &&
                b.PackageId == packageId &&
                b.Status != "Completed" &&
                b.Status != "Cancelled");
        }

        public async Task<(bool success, string? error)> CompletePaymentAsync(Guid bookingId, Guid userId, string paymentReference)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null || booking.UserId != userId)
                return (false, "Booking not found.");

            if (booking.Status != "PendingPayment")
                return (false, "This booking is not awaiting payment.");

            booking.Status = "Confirmed";
            booking.PaymentStatus = "Paid";
            booking.PaymentReference = paymentReference;
            booking.PaidAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return (true, null);
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

            if (booking.Status == "PendingPayment")
            {
                // No payment has been captured yet, so it can be abandoned freely.
                booking.Status = "Cancelled";
                booking.CancelledAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return (true, null);
            }

            if (booking.Status != "Confirmed")
                return (false, "Only confirmed bookings can be cancelled.");

            if (booking.Package == null || booking.Package.TourStartDate - DateTime.Now < CancellationWindow)
                return (false, "Bookings can only be cancelled at least 3 days before the tour start date.");

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> AdminUpdateTravelersAndPriceAsync(Guid bookingId, int travelersCount, decimal? totalPrice)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            booking.TravelersCount = travelersCount;
            booking.TotalPrice = totalPrice;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> AdminAddTravelersAsync(Guid bookingId, List<BookingTravelers> newTravelers, int newCount, decimal? newPrice)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            foreach (var traveler in newTravelers)
            {
                traveler.Id = Guid.NewGuid();
                traveler.BookingId = bookingId;
            }

            await _context.BookingTravelers.AddRangeAsync(newTravelers);

            booking.TravelersCount = newCount;
            booking.TotalPrice = newPrice;
            booking.AreTravelerDetailsSubmitted = true;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error, int remainingCount)> RemoveTravelerAsync(Guid bookingId, Guid travelerId, int newCount, decimal? newPrice)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingTravelers)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
                return (false, "Booking not found.", 0);

            var traveler = booking.BookingTravelers?.FirstOrDefault(t => t.Id == travelerId);
            if (traveler == null)
                return (false, "Traveler not found.", booking.BookingTravelers?.Count ?? 0);

            _context.BookingTravelers.Remove(traveler);
            booking.BookingTravelers!.Remove(traveler);

            var remaining = booking.BookingTravelers.Count;

            if (remaining <= newCount)
            {
                booking.TravelersCount = newCount;
                booking.TotalPrice = newPrice;
            }

            await _context.SaveChangesAsync();
            return (true, null, remaining);
        }

        public async Task<(bool success, string? error)> AdminConfirmAsync(Guid bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status != "PendingPayment")
                return (false, "Only bookings awaiting payment confirmation can be confirmed.");

            if (booking.PaymentStatus != "Paid")
                return (false, "This booking cannot be confirmed until payment has been received.");

            booking.Status = "Confirmed";
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> AdminCompleteAsync(Guid bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status != "Confirmed")
                return (false, "Only confirmed bookings can be marked as completed.");

            booking.Status = "Completed";
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> AdminCancelAsync(Guid bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.Status == "Completed")
                return (false, "Completed bookings cannot be cancelled.");

            if (booking.Status == "Cancelled")
                return (false, "This booking is already cancelled.");

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool success, string? error)> AdminMarkPaidAsync(Guid bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null)
                return (false, "Booking not found.");

            if (booking.PaymentStatus == "Paid")
                return (false, "This booking is already marked as paid.");

            booking.PaymentStatus = "Paid";
            booking.PaidAt = DateTime.Now;

            if (string.IsNullOrWhiteSpace(booking.PaymentReference))
                booking.PaymentReference = $"ADMIN-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";

            await _context.SaveChangesAsync();
            return (true, null);
        }
    }
}
