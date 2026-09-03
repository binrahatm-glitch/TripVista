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
        private readonly IReportingDAO _reportingDAO;
        private readonly IBudgetModelTrainer _budgetModelTrainer;
        private readonly IBookingDAO _bookingDAO;

        public AdminController(IPackageDAO packageDAO, IDestinationDAO destinationDAO, IReportingDAO reportingDAO, IBudgetModelTrainer budgetModelTrainer, IBookingDAO bookingDAO)
        {
            _packageDAO = packageDAO;
            _destinationDAO = destinationDAO;
            _reportingDAO = reportingDAO;
            _budgetModelTrainer = budgetModelTrainer;
            _bookingDAO = bookingDAO;
        }

        public async Task<IActionResult> AdminDashboard()
        {
            var stats = await _reportingDAO.GetDashboardStatsAsync();
            return View(stats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetrainAiModels()
        {
            var summary = await _budgetModelTrainer.RetrainAsync();

            var dataSource = summary.PackagePriceModelUsedRealData ? "real package data" : "synthetic data (not enough real packages yet)";
            TempData["msg"] = $"AI models retrained. Price model: {summary.PackagePriceTrainingRows} rows ({dataSource}). Tier model: {summary.TripTierTrainingRows} rows (synthetic).";
            return RedirectToAction("AdminDashboard");
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
            ValidatePackageDates(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var adminId = Guid.Parse(HttpContext.Session.GetString("id")!);

            var package = new Packages
            {
                PackageId = Guid.NewGuid(),
                Title = vm.Title,
                Details = vm.Details,
                ImagePath = await SaveUploadedImageAsync(vm.ImageFile) ?? vm.ImagePath,
                Region = vm.Region,
                PricePerPerson = vm.PricePerPerson,
                DurationDays = (vm.TourEndDate.Date - vm.TourStartDate.Date).Days,
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

        private void ValidatePackageDates(PackageFormViewModel vm)
        {
            if (vm.TourStartDate.Date < DateTime.Today)
            {
                ModelState.AddModelError(nameof(vm.TourStartDate), "Tour start date must be today or in the future.");
            }

            if (vm.TourEndDate.Date < vm.TourStartDate.Date)
            {
                ModelState.AddModelError(nameof(vm.TourEndDate), "Tour end date must be on or after the start date.");
            }
        }

        private async Task<string?> SaveUploadedImageAsync(IFormFile? image)
        {
            if (image == null || image.Length == 0) return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return $"/images/{fileName}";
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
                Region = package.Region ?? string.Empty,
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

            ValidatePackageDates(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var package = new Packages
            {
                PackageId = vm.PackageId.Value,
                Title = vm.Title,
                Details = vm.Details,
                ImagePath = await SaveUploadedImageAsync(vm.ImageFile) ?? vm.ImagePath,
                Region = vm.Region,
                PricePerPerson = vm.PricePerPerson,
                DurationDays = (vm.TourEndDate.Date - vm.TourStartDate.Date).Days,
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

            List<Destinations> DuplicateByName = await _destinationDAO.SearchAsyncByName(dest.Name);
            List<Destinations> DuplicateBylocation = await _destinationDAO.SearchAsyncByLocation(dest.Location);


            if (DuplicateByName.Count > 0)
            {
                ModelState.AddModelError("Name", "A destination with this name already exists.");
                return View(dest);
            }

            if (DuplicateBylocation.Count > 0)
            {
                ModelState.AddModelError("Location", "A destination with this location already exists.");
                return View(dest);
            }


            var imageUrl = await SaveUploadedImageAsync(dest.Image);

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
            return RedirectToAction("ManageDestinations");
        }

        [HttpGet]
        public async Task<IActionResult> DestinationQueue()
        {
            var pending = await _destinationDAO.GetPendingAsync();
            return View(pending);
        }

        [HttpGet]
        public async Task<IActionResult> ManageDestinations()
        {
            var reviewed = await _destinationDAO.GetReviewedAsync();
            return View(reviewed);
        }

        [HttpGet]
        public async Task<IActionResult> EditDestination(Guid id)
        {
            var destination = await _destinationDAO.GetByIdAsync(id);
            if (destination == null) return NotFound();

            var vm = new EditDestinationViewModel
            {
                DestinationId = destination.DestinationId,
                Name = destination.Name,
                Location = destination.Location,
                Description = destination.Description,
                ImageUrl = destination.ImageUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDestination(EditDestinationViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var destination = new Destinations
            {
                DestinationId = vm.DestinationId,
                Name = vm.Name,
                Location = vm.Location,
                Description = vm.Description,
                ImageUrl = await SaveUploadedImageAsync(vm.Image) ?? vm.ImageUrl
            };

            await _destinationDAO.UpdateAsync(destination);
            TempData["msg"] = "Destination updated.";
            return RedirectToAction("ManageDestinations");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDestination(Guid id)
        {
            var deleted = await _destinationDAO.DeleteAsync(id);
            TempData[deleted ? "msg" : "Error"] = deleted
                ? "Destination deleted."
                : "This destination cannot be deleted because it is linked to one or more packages.";
            return RedirectToAction("ManageDestinations");
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

        // ------------------- Bookings -------------------
        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            List<Bookings> bookings = await _bookingDAO.GetBookings();


            return View(bookings);

        }
        [HttpGet]
        public async Task<IActionResult> GetBookingDetails(Guid bookingId)
        {
            var booking = await _bookingDAO.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
    }
}
