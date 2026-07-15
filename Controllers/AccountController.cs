using Microsoft.AspNetCore.Mvc;
using TripVista.Models.DTO;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class AccountController : Controller
    {
        private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan OtpResendCooldown = TimeSpan.FromSeconds(60);

        private readonly IUserDAO _userService;
        private readonly IOtpDAO _otpService;
        private readonly IEmailService _emailService;

        public AccountController(IUserDAO userService, IOtpDAO otpService, IEmailService emailService)
        {
            _userService = userService;
            _otpService = otpService;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Users? existingUser = await _userService.ValidateLoginAsync(model.Email, model.Password);

            if (existingUser == null)
            {
                ViewBag.Error = "Incorrect Credentials";
                return View(model);
            }

            if (!existingUser.IsEmailVerified)
            {
                ViewBag.Error = "Please verify your email before logging in.";
                return View(model);
            }

            HttpContext.Session.SetString("name", existingUser.FullName ?? string.Empty);
            HttpContext.Session.SetString("id", existingUser.UserId.ToString());
            HttpContext.Session.SetString("email", existingUser.Email);
            HttpContext.Session.SetString("role", existingUser.Role);

            if (string.Equals(existingUser.Role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("AdminDashboard", "Admin");
            }

            return RedirectToAction("UserDashboard", "User");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            Users? existing = await _userService.GetUserByEmailAsync(model.Email);
            if (existing != null)
            {
                ViewBag.Error = "Email already exists";
                return View(model);
            }

            await _userService.CreateUserAsync(new Users
            {
                UserId = Guid.NewGuid(),
                FullName = model.FullName,
                Email = model.Email,
                Password = model.Password, // hashed inside CreateUserAsync
                Role = "user",
                IsEmailVerified = false
            });

            await IssueOtpAsync(model.Email);

            return RedirectToAction("VerifyEmail", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            return View(new EmailVerifyViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(EmailVerifyViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.OtpCode))
            {
                ViewBag.Error = "Invalid request.";
                return View(model);
            }

            var validOtp = await _otpService.GetValidAsync(model.Email, model.OtpCode);
            if (validOtp == null)
            {
                ViewBag.Error = "Invalid or expired OTP. Try again.";
                return View(model);
            }

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                ViewBag.Error = "Registration data not found. Please register again.";
                return RedirectToAction("Register");
            }

            await _otpService.MarkUsedAsync(validOtp);
            user.IsEmailVerified = true;
            await _userService.UpdateUserAsync(user);

            TempData["msg"] = "Email verified successfully! You can now log in.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("Register");

            var latest = await _otpService.GetLatestAsync(email);
            if (latest != null && DateTime.Now - latest.CreatedAt < OtpResendCooldown)
            {
                var wait = OtpResendCooldown - (DateTime.Now - latest.CreatedAt);
                TempData["Error"] = $"Please wait {Math.Ceiling(wait.TotalSeconds)}s before requesting another OTP.";
                return RedirectToAction("VerifyEmail", new { email });
            }

            await IssueOtpAsync(email);
            TempData["msg"] = "A new OTP has been sent to your email.";
            return RedirectToAction("VerifyEmail", new { email });
        }

        private async Task IssueOtpAsync(string email)
        {
            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            await _otpService.CreateAsync(email, otpCode, OtpValidity);
            await _emailService.SendAsync(email, "Your TripVista Email Verification Code", $"Your OTP Code is: {otpCode}. It expires in 10 minutes.");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
