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
        public async Task<IActionResult> Payment(Guid bookingId)
        {
            var booking = await _bookingDAO.GetByIdAsync(bookingId);
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            if (booking.Status != "PendingPayment")
            {
                TempData["msg"] = "This booking has already been paid for.";
                return RedirectToAction("Bookings");
            }

            var vm = new PaymentViewModel
            {
                BookingId = booking.BookingId,
                PackageTitle = booking.Package?.Title ?? string.Empty,
                TravelersCount = booking.TravelersCount,
                Amount = booking.TotalPrice ?? 0
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(PaymentViewModel vm)
        {
            var booking = await _bookingDAO.GetByIdAsync(vm.BookingId);
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);

            if (booking == null || booking.UserId != userId)
            {
                return NotFound();
            }

            if (booking.Status != "PendingPayment")
            {
                TempData["msg"] = "This booking has already been paid for.";
                return RedirectToAction("Bookings");
            }

            if (!ModelState.IsValid)
            {
                vm.PackageTitle = booking.Package?.Title ?? string.Empty;
                vm.TravelersCount = booking.TravelersCount;
                vm.Amount = booking.TotalPrice ?? 0;
                return View(vm);
            }

            // Demo payment gateway: no card data is stored, we just simulate a successful charge.
            var paymentReference = $"DEMO-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
            var (success, error) = await _bookingDAO.CompletePaymentAsync(booking.BookingId, userId, paymentReference);

            if (!success)
            {
                TempData["Error"] = error;
                return RedirectToAction("Bookings");
            }

            TempData["msg"] = "Payment successful! Your booking is now confirmed.";
            return RedirectToAction("AddTravelerDetails", new { bookingId = booking.BookingId });
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

            if (booking.Status == "PendingPayment")
            {
                return RedirectToAction("Payment", new { bookingId = booking.BookingId });
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
                CNICOrPassportNumber = t.CNICOrPassportNumber,
                ContactNumber = t.ContactNumber
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
