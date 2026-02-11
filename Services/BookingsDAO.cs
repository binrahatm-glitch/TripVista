using Microsoft.EntityFrameworkCore;
using TripVista.Data;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Services
{
    public class BookingsDAO : IBookingDAO
    {
        private readonly SmartTripContext _context;

        public BookingsDAO(SmartTripContext context)
        {
            _context = context;
        }

        public IEnumerable<Bookings> GetAll()
        {
            return _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Package)
                .ToList();
        }

        public Bookings? GetById(Guid id)
        {
            return _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Package)
                .FirstOrDefault(b => b.BookingId == id);
        }

        public IEnumerable<Bookings> GetByUserId(Guid userId)
        {
            return _context.Bookings
                .Include(b => b.Package)
                .Where(b => b.UserId == userId)
                .ToList();
        }

        public void Create(Bookings booking)
        {
            _context.Bookings.Add(booking);
        }

        public void Update(Bookings booking)
        {
            _context.Bookings.Update(booking);
        }

        public void Delete(Guid id)
        {
            var booking = GetById(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
