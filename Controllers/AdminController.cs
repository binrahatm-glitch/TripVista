using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Models.DTO;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    [RequireRole("admin")]
    public class AdminController : Controller
    {
        private readonly IPackageDAO _packageDAO;
        private readonly IDestinationDAO _destinationDAO;

        public AdminController(IPackageDAO packageDAO, IDestinationDAO destinationDAO)
        {
            _packageDAO = packageDAO;
            _destinationDAO = destinationDAO;
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        // ---------------- Packages ----------------

        [HttpGet]
        public async Task<IActionResult> Packages()
        {
            var packages = await _packageDAO.GetAllAsync();
            return View(packages);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePackage()
        {
            var vm = new PackageFormViewModel
            {
                DestinationOptions = await BuildDestinationOptionsAsync(null)
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePackage(PackageFormViewModel vm)
        {
            if (vm.TourEndDate <= vm.TourStartDate)
            {
                ModelState.AddModelError(nameof(vm.TourEndDate), "Tour end date must be after the start date.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var adminId = Guid.Parse(HttpContext.Session.GetString("id")!);

            var package = new Packages
            {
                PackageId = Guid.NewGuid(),
                Title = vm.Title,
                Details = vm.Details,
                ImagePath = vm.ImagePath,
                PricePerPerson = vm.PricePerPerson,
                DurationDays = vm.DurationDays,
                MaxGroupSize = vm.MaxGroupSize,
                TourStartDate = vm.TourStartDate,
                TourEndDate = vm.TourEndDate,
                IsActive = vm.IsActive,
                CreatedByAdminId = adminId,
                CreatedAt = DateTime.Now
            };

            var destinationIds = vm.DestinationOptions.Where(d => d.Selected).Select(d => d.DestinationId).ToList();
            await _packageDAO.CreateAsync(package, destinationIds);
            TempData["msg"] = "Package created successfully.";
            return RedirectToAction("Packages");
        }

        [HttpGet]
        public async Task<IActionResult> EditPackage(Guid id)
        {
            var package = await _packageDAO.GetByIdAsync(id);
            if (package == null) return NotFound();

            var selectedIds = package.PackageDestinations?.Select(pd => pd.DestinationId).ToHashSet() ?? new HashSet<Guid>();

            var vm = new PackageFormViewModel
            {
                PackageId = package.PackageId,
                Title = package.Title,
                Details = package.Details,
                ImagePath = package.ImagePath,
                PricePerPerson = package.PricePerPerson,
                DurationDays = package.DurationDays,
                MaxGroupSize = package.MaxGroupSize,
                TourStartDate = package.TourStartDate,
                TourEndDate = package.TourEndDate,
                IsActive = package.IsActive,
                DestinationOptions = await BuildDestinationOptionsAsync(selectedIds)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPackage(PackageFormViewModel vm)
        {
            if (vm.PackageId == null) return NotFound();

            if (vm.TourEndDate <= vm.TourStartDate)
            {
                ModelState.AddModelError(nameof(vm.TourEndDate), "Tour end date must be after the start date.");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var package = new Packages
            {
                PackageId = vm.PackageId.Value,
                Title = vm.Title,
                Details = vm.Details,
                ImagePath = vm.ImagePath,
                PricePerPerson = vm.PricePerPerson,
                DurationDays = vm.DurationDays,
                MaxGroupSize = vm.MaxGroupSize,
                TourStartDate = vm.TourStartDate,
                TourEndDate = vm.TourEndDate,
                IsActive = vm.IsActive
            };

            var destinationIds = vm.DestinationOptions.Where(d => d.Selected).Select(d => d.DestinationId).ToList();
            await _packageDAO.UpdateAsync(package, destinationIds);
            TempData["msg"] = "Package updated successfully.";
            return RedirectToAction("Packages");
        }

        private async Task<List<PackageDestinationOption>> BuildDestinationOptionsAsync(HashSet<Guid>? selectedIds)
        {
            var destinations = await _destinationDAO.GetApprovedAsync();
            return destinations
                .OrderBy(d => d.Name)
                .Select(d => new PackageDestinationOption
                {
                    DestinationId = d.DestinationId,
                    Name = d.Name,
                    Selected = selectedIds?.Contains(d.DestinationId) ?? false
                })
                .ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivatePackage(Guid id)
        {
            await _packageDAO.DeactivateAsync(id);
            TempData["msg"] = "Package deactivated.";
            return RedirectToAction("Packages");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePackage(Guid id)
        {
            var deleted = await _packageDAO.DeleteAsync(id);
            TempData[deleted ? "msg" : "Error"] = deleted
                ? "Package deleted."
                : "This package cannot be deleted because it has existing bookings. Deactivate it instead.";
            return RedirectToAction("Packages");
        }

        // ---------------- Destination review ----------------

        [HttpGet]
        public IActionResult CreateDestination()
        {
            return View(new AddDestinationDTO());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDestination(AddDestinationDTO dest)
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

            var adminId = Guid.Parse(HttpContext.Session.GetString("id")!);

            var newDestination = new Destinations
            {
                DestinationId = Guid.NewGuid(),
                Name = dest.Name,
                Location = dest.Location,
                Description = dest.Description,
                ImageUrl = imageUrl,
                Status = "Approved",
                SubmittedByUserId = adminId,
                SubmittedAt = DateTime.Now,
                ReviewedByAdminId = adminId,
                ReviewedAt = DateTime.Now
            };

            await _destinationDAO.CreateAsync(newDestination);
            TempData["msg"] = "Destination created and published.";
            return RedirectToAction("DestinationHistory");
        }

        [HttpGet]
        public async Task<IActionResult> DestinationQueue()
        {
            var pending = await _destinationDAO.GetPendingAsync();
            return View(pending);
        }

        [HttpGet]
        public async Task<IActionResult> DestinationHistory()
        {
            var reviewed = await _destinationDAO.GetReviewedAsync();
            return View(reviewed);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveDestination(Guid id, string? remarks)
        {
            var adminId = Guid.Parse(HttpContext.Session.GetString("id")!);
            await _destinationDAO.ApproveAsync(id, adminId, remarks);
            TempData["msg"] = "Destination approved.";
            return RedirectToAction("DestinationQueue");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectDestination(Guid id, string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                TempData["Error"] = "Remarks are required when rejecting a destination.";
                return RedirectToAction("DestinationQueue");
            }

            var adminId = Guid.Parse(HttpContext.Session.GetString("id")!);
            await _destinationDAO.RejectAsync(id, adminId, remarks);
            TempData["msg"] = "Destination rejected.";
            return RedirectToAction("DestinationQueue");
        }
    }
}
