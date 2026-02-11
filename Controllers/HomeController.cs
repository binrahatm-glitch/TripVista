using Microsoft.AspNetCore.Mvc;

namespace TripVista.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
