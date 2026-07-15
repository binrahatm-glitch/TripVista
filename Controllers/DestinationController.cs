using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Models.DTO;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    [RequireRole("user", "admin")]
    public class DestinationController : Controller
    {
        private readonly IDestinationDAO _destinationDAO;

        public DestinationController(IDestinationDAO destinationDAO)
        {
            _destinationDAO = destinationDAO;
        }

        [HttpGet]
        public async Task<IActionResult> Destinations()
        {
            var destinations = await _destinationDAO.GetApprovedAsync();
            return View(destinations);
        }

        [HttpGet]
        public async Task<IActionResult> ViewDetails(Guid id)
        {
            var destination = await _destinationDAO.GetByIdAsync(id);
            if (destination == null || destination.Status != "Approved")
            {
                return NotFound();
            }

            return View(destination);
        }

        [HttpGet]
        [RequireRole("user")]
        public IActionResult AddDestination()
        {
            return View();
        }

        [HttpPost]
        [RequireRole("user")]
        public async Task<IActionResult> AddDestination(AddDestinationDTO dest)
        {
            if (!ModelState.IsValid)
            {
                return View(dest);
            }

            string? imageUrl = null;
            if (dest.Image != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dest.Image.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await dest.Image.CopyToAsync(stream);

                imageUrl = $"/images/{fileName}";
            }

            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);

            var newDestination = new Destinations
            {
                DestinationId = Guid.NewGuid(),
                Name = dest.Name,
                Location = dest.Location,
                Description = dest.Description,
                ImageUrl = imageUrl,
                Status = "Pending",
                SubmittedByUserId = userId,
                SubmittedAt = DateTime.Now
            };

            await _destinationDAO.CreateAsync(newDestination);

            return RedirectToAction("SubmissionResponse", new { id = newDestination.DestinationId });
        }

        [HttpGet]
        [RequireRole("user")]
        public async Task<IActionResult> SubmissionResponse(Guid id)
        {
            var submission = await _destinationDAO.GetByIdAsync(id);
            if (submission == null) return NotFound();

            return View(submission);
        }

        [HttpGet]
        [RequireRole("user")]
        public async Task<IActionResult> MySubmissions()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);
            var submissions = await _destinationDAO.GetByUserIdAsync(userId);
            return View(submissions);
        }
    }
}
