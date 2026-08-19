using Microsoft.AspNetCore.Mvc;
using TripVista.Filters;
using TripVista.Models.DTO;
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

        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);
            var user = await _userDAO.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            var vm = new UpdateProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                AvatarPath = user.AvatarPath
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileViewModel vm)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);

            if (!ModelState.IsValid)
                return View(vm);

            string? avatarPath = null;
            if (vm.Avatar != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Avatar.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await vm.Avatar.CopyToAsync(stream);

                avatarPath = $"/images/{fileName}";
            }

            await _userDAO.UpdateProfileAsync(userId, vm.FullName, avatarPath);

            HttpContext.Session.SetString("name", vm.FullName);

            TempData["msg"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            var userId = Guid.Parse(HttpContext.Session.GetString("id")!);
            var user = await _userDAO.GetUserByIdAsync(userId);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please correct the highlighted password fields.";
                return RedirectToAction("UpdateProfile");
            }

            var validated = await _userDAO.ValidateLoginAsync(user.Email, vm.CurrentPassword);
            if (validated == null)
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("UpdateProfile");
            }

            await _userDAO.ChangePasswordAsync(userId, vm.NewPassword);

            TempData["msg"] = "Password changed successfully.";
            return RedirectToAction("UpdateProfile");
        }
    }
}
