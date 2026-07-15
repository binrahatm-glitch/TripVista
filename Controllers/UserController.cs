using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    [RequireRole("user")]
    public class UserController : Controller
    {
        private readonly IPackageDAO _packageDAO;
        private readonly IUserDAO _userDAO;

        public UserController(IPackageDAO packageDAO, IUserDAO userDAO)
        {
            _packageDAO = packageDAO;
            _userDAO = userDAO;
        }

        [HttpGet]
        public async Task<IActionResult> UserDashboard()
        {
            var packages = await _packageDAO.GetActiveAsync();
            return View(packages);
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id") ?? string.Empty);
            var user = await _userDAO.GetUserByIdAsync(userId);
            return View(user);
        }
    }
}
