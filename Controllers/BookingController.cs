using Microsoft.AspNetCore.Mvc;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingDAO _bookingDAO;
        public BookingController(IBookingDAO bookingDAO) 
        {
            _bookingDAO = bookingDAO;
        }
        public IActionResult Bookings()
        {
            Guid id = Guid.Parse(HttpContext.Session.GetString("id") ?? string.Empty);
            List<Bookings> bookings = _bookingDAO.GetByUserId(id).ToList();
            return View(bookings);
        }
    }
}
