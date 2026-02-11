using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class UserController : Controller
    {
        private readonly IPackageDAO _packageDAO;
        private readonly IDestinationDAO _destinationDAO;
        private readonly IUserDAO _userDAO;

        public UserController(IPackageDAO packageDAO, IDestinationDAO destinationDAO, IUserDAO userDAO)
        {
            _packageDAO = packageDAO;
            _destinationDAO = destinationDAO;
            _userDAO = userDAO;
        }

        [HttpGet]
        public async Task<IActionResult> UserDashboard()
        {
            
            var packages = await _packageDAO.GetAllPackagesWithDestinationsAsync();


            return View(packages);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            Users? user = _userDAO.GetUserByIdAsync(Guid.Parse(HttpContext.Session.GetString("id") ?? string.Empty)).Result;
            return View(user);
        }
    }

}
