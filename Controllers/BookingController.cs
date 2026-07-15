using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Models.DTO;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    [RequireRole("user")]
    public class BookingController : Controller
    {
        private readonly IBookingDAO _bookingDAO;

        public BookingController(IBookingDAO bookingDAO)
        {
            _bookingDAO = bookingDAO;
        }

        [HttpGet]
        public async Task<IActionResult> Bookings()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);
            var bookings = await _bookingDAO.GetByUserIdAsync(userId);
            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> AddTravelerDetails(Guid bookingId)
        {
            var booking = await _bookingDAO.GetByIdAsync(bookingId);
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            var vm = new AddTravelerDetailsViewModel
            {
                BookingId = booking.BookingId,
                PackageTitle = booking.Package?.Title ?? string.Empty,
                Travelers = Enumerable.Range(0, booking.TravelersCount)
                    .Select(_ => new TravelerDetailInput())
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTravelerDetails(AddTravelerDetailsViewModel vm)
        {
            var booking = await _bookingDAO.GetByIdAsync(vm.BookingId);
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid || vm.Travelers.Count != booking.TravelersCount)
            {
                vm.PackageTitle = booking.Package?.Title ?? string.Empty;
                return View(vm);
            }

            var travelers = vm.Travelers.Select(t => new BookingTravelers
            {
                FullName = t.FullName,
                Age = t.Age,
                CNICOrPassportNumber = t.CNICOrPassportNumber
            }).ToList();

            await _bookingDAO.AddTravelerDetailsAsync(booking.BookingId, travelers);

            TempData["msg"] = "Traveler details saved successfully.";
            return RedirectToAction("Bookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid bookingId)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);
            var (success, error) = await _bookingDAO.CancelAsync(bookingId, userId);

            TempData[success ? "msg" : "Error"] = success ? "Booking cancelled successfully." : error;
            return RedirectToAction("Bookings");
        }
    }
}
