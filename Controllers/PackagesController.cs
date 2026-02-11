using Microsoft.AspNetCore.Mvc;
using TripVista.Data;
using TripVista.Models.DTO;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class PackagesController : Controller
    {
       
        private readonly IPackageDAO _packageDAO;
        private readonly IUserDAO _userDAO;
        private readonly IBookingDAO _bookingDAO;
        
        public PackagesController(IPackageDAO packageDAO, IUserDAO userDAO, IBookingDAO bookingDAO)
        {
            _packageDAO = packageDAO;
            _userDAO = userDAO;
            _bookingDAO = bookingDAO;
        }

        [HttpGet]
        public IActionResult ViewDetails(Guid id)
        {
            var package = _packageDAO.GetById(id);

            if (package == null)
            {
                return NotFound();
            }

            return View(package);
        }

        [HttpGet]
        public IActionResult AddBooking(Guid id)
        {
            Packages? package = _packageDAO.GetById(id);

            if (package == null)
            {
                return NotFound();
            }

            string email = HttpContext.Session.GetString("email") ?? "";
            Users? user = _userDAO.GetUserByEmailAsync(email).Result;

            if (user == null)
            {
                return NotFound();
            }

            AddBookingViewModel vm = new AddBookingViewModel
            {
                PackageId = package.PackageId,
                PackageTitle = package.Title,
                Price = package.Price ?? 0,
                Discount = package.Discount,
                Email = user.Email,
                FullName = user.FullName ?? "",
                UserId = user.UserId

            };




            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddBooking(AddBookingViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var finalPrice = (vm.Price - vm.Discount) * vm.NumberOfPersons;

            var booking = new Bookings
            {
                BookingId = Guid.NewGuid(),
                PackageId = vm.PackageId,
                UserId = vm.UserId,
                TravelDate = vm.TravelDate,
                TravelersCount = vm.NumberOfPersons,
                TotalPrice = finalPrice
            };

            _bookingDAO.Create(booking);
            _bookingDAO.Save();

            return RedirectToAction("Confirmation");
        }

        public IActionResult Confirmation()
        {
            return View();
        }
    }
}
