using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Models.DTO;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class PackagesController : Controller
    {
        private readonly IPackageDAO _packageDAO;
        private readonly IBookingDAO _bookingDAO;

        public PackagesController(IPackageDAO packageDAO, IBookingDAO bookingDAO)
        {
            _packageDAO = packageDAO;
            _bookingDAO = bookingDAO;
        }

        [HttpGet]
        public async Task<IActionResult> ViewDetails(Guid id)
        {
            var package = await _packageDAO.GetByIdAsync(id);
            if (package == null)
            {
                return NotFound();
            }

            return View(package);
        }

        [HttpGet]
        [RequireRole("user")]
        public async Task<IActionResult> AddBooking(Guid id)
        {
            var package = await _packageDAO.GetByIdAsync(id);
            if (package == null || !package.IsActive)
            {
                return NotFound();
            }

            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);
            if (await _bookingDAO.HasActiveBookingAsync(userId, id))
            {
                TempData["Error"] = "You already have an active booking for this package.";
                return RedirectToAction("ViewDetails", new { id });
            }

            var vm = new AddBookingViewModel
            {
                PackageId = package.PackageId,
                PackageTitle = package.Title,
                PricePerPerson = package.PricePerPerson,
                MaxGroupSize = package.MaxGroupSize,
                NumberOfTravelers = 1
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRole("user")]
        public async Task<IActionResult> AddBooking(AddBookingViewModel vm)
        {
            var package = await _packageDAO.GetByIdAsync(vm.PackageId);
            if (package == null || !package.IsActive)
            {
                return NotFound();
            }

            vm.PackageTitle = package.Title;
            vm.PricePerPerson = package.PricePerPerson;
            vm.MaxGroupSize = package.MaxGroupSize;

            if (!ModelState.IsValid)
                return View(vm);

            if (vm.NumberOfTravelers > package.MaxGroupSize)
            {
                ModelState.AddModelError(nameof(vm.NumberOfTravelers), $"This package allows a maximum of {package.MaxGroupSize} travelers.");
                return View(vm);
            }

            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);

            if (await _bookingDAO.HasActiveBookingAsync(userId, package.PackageId))
            {
                ModelState.AddModelError(string.Empty, "You already have an active booking for this package.");
                return View(vm);
            }

            var totalPrice = package.PricePerPerson * vm.NumberOfTravelers;

            var booking = await _bookingDAO.CreatePendingBookingAsync(userId, package.PackageId, vm.NumberOfTravelers, totalPrice);

            return RedirectToAction("Payment", "Booking", new { bookingId = booking.BookingId });
        }
    }
}
