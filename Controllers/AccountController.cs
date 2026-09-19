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

            await IssueOtpAsync(model.Email, OtpPurpose.Registration);

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

            var validOtp = await _otpService.GetValidAsync(model.Email, model.OtpCode, OtpPurpose.Registration);
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

            var latest = await _otpService.GetLatestAsync(email, OtpPurpose.Registration);
            if (latest != null && DateTime.UtcNow - latest.CreatedAt < OtpResendCooldown)
            {
                var wait = OtpResendCooldown - (DateTime.UtcNow - latest.CreatedAt);
                TempData["Error"] = $"Please wait {Math.Ceiling(wait.TotalSeconds)}s before requesting another OTP.";
                return RedirectToAction("VerifyEmail", new { email });
            }

            await IssueOtpAsync(email, OtpPurpose.Registration);
            TempData["msg"] = "A new OTP has been sent to your email.";
            return RedirectToAction("VerifyEmail", new { email });
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user != null)
            {
                await IssueOtpAsync(model.Email, OtpPurpose.PasswordReset);
            }

            // Same message whether or not the account exists, so we don't leak which emails are registered.
            TempData["msg"] = "If that email is registered, a reset code has been sent to it.";
            return RedirectToAction("ResetPassword", new { email = model.Email });
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var validOtp = await _otpService.GetValidAsync(model.Email, model.OtpCode, OtpPurpose.PasswordReset);
            if (validOtp == null)
            {
                ViewBag.Error = "Invalid or expired code. Please try again.";
                return View(model);
            }

            var user = await _userService.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                ViewBag.Error = "Invalid request.";
                return View(model);
            }

            await _otpService.MarkUsedAsync(validOtp);
            await _userService.ChangePasswordAsync(user.UserId, model.NewPassword);

            TempData["msg"] = "Password reset successfully. You can now log in.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendResetOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return RedirectToAction("ForgotPassword");

            var latest = await _otpService.GetLatestAsync(email, OtpPurpose.PasswordReset);
            if (latest != null && DateTime.UtcNow - latest.CreatedAt < OtpResendCooldown)
            {
                var wait = OtpResendCooldown - (DateTime.UtcNow - latest.CreatedAt);
                TempData["Error"] = $"Please wait {Math.Ceiling(wait.TotalSeconds)}s before requesting another code.";
                return RedirectToAction("ResetPassword", new { email });
            }

            var user = await _userService.GetUserByEmailAsync(email);
            if (user != null)
            {
                await IssueOtpAsync(email, OtpPurpose.PasswordReset);
            }

            TempData["msg"] = "If that email is registered, a new code has been sent to it.";
            return RedirectToAction("ResetPassword", new { email });
        }

        private async Task IssueOtpAsync(string email, string purpose)
        {
            var otpCode = Random.Shared.Next(100000, 999999).ToString();
            await _otpService.CreateAsync(email, otpCode, OtpValidity, purpose);

            var (subject, body) = purpose == OtpPurpose.PasswordReset
                ? ("Your TripVista Password Reset Code", $"Your password reset code is: {otpCode}. It expires in 10 minutes. If you didn't request this, you can ignore this email.")
                : ("Your TripVista Email Verification Code", $"Your OTP Code is: {otpCode}. It expires in 10 minutes.");

            await _emailService.SendAsync(email, subject, body);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
