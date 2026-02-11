using Microsoft.AspNetCore.Mvc;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class UserSubmissionController : Controller
    {
        private readonly IUserSubmissionDAO _userSubmissionDAO;
        public UserSubmissionController(IUserSubmissionDAO userSubmissionDAO)
        {
            _userSubmissionDAO = userSubmissionDAO;
        }
        public IActionResult Submissions()
        {
            Guid id = Guid.Parse(HttpContext.Session.GetString("id") ?? string.Empty);
            List<UserSubmissions> submissions = _userSubmissionDAO.GetByUserId(id).ToList();
            return View(submissions);
        }
    }
}
