using Microsoft.AspNetCore.Mvc;

namespace TripVista.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult AdminDashboard()
        {
            return View();
        }
    }
}
