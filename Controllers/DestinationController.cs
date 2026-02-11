using Microsoft.AspNetCore.Mvc;
using TripVista.Data;
using TripVista.Models.DTO;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class DestinationController : Controller
    {
        private readonly IDestinationDAO _destinationDAO;
        private readonly IUserSubmissionDAO _userSubmissionDAO;

        public DestinationController(IDestinationDAO destinationDAO, IUserSubmissionDAO userSubmissionDAO)
        {
            _destinationDAO = destinationDAO;
            _userSubmissionDAO = userSubmissionDAO;
        }

        [HttpGet]
        public IActionResult ViewDetails(Guid id)
        {
            var destination = _destinationDAO.GetById(id);

            if (destination == null)
            {
                return NotFound(); 
            }

            return View(destination);
        }
        [HttpGet]
        public IActionResult AddDestination()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Destinations()
        {
            List<Destinations> destinations = _destinationDAO.GetAll();
            return View(destinations);
        }


        [HttpPost]
        public async Task<IActionResult> AddDestination(AddDestinationDTO dest)
        {
            if (ModelState.IsValid)
            {
                string? ImagePath = null;

                if (dest.Image != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dest.Image.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await dest.Image.CopyToAsync(stream);

                    ImagePath = $"/images/{fileName}";
                }
                var newDestination = new UserSubmissions
                {
                    Name = dest.Name,
                    Category = dest.Category,
                    Location = dest.Location,
                    Description = dest.Description,
                    ImagePath = ImagePath,
                    Status = "Pending",
                    UserId = HttpContext.Session.GetString("id") != null ? Guid.Parse(HttpContext.Session.GetString("id")!) : Guid.Empty,
                    CreatedAt = DateTime.UtcNow
                };
                _userSubmissionDAO.Create(newDestination);
                _userSubmissionDAO.Save();
                return RedirectToAction("SubmissionResponse", new { id = newDestination.SubmissionId });
            }
            return View(dest);
        }

        [HttpGet]
        public IActionResult SubmissionResponse(Guid id)
        {
            UserSubmissions? submission = _userSubmissionDAO.GetById(id);
            return View(submission);
        }
    }
}
