using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using TripVista.Models.DTO;
using TripVista.Models.Entities;
using TripVista.Services.Interfaces;

namespace TripVista.Controllers
{
    public class AccountController : Controller
    {
        
        private readonly IUserDAO _userService;
        public AccountController(IUserDAO userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            Users? existingUser =  await _userService.ValidateLoginAsync(user.Email, user.Password);

            if (existingUser != null)
            {
                HttpContext.Session.SetString("name", existingUser.FullName?.ToString() ?? string.Empty);
                HttpContext.Session.SetString("id", existingUser.UserId.ToString() ?? string.Empty);
                HttpContext.Session.SetString("email", existingUser.Email?.ToString() ?? string.Empty);
                if (existingUser.Role == "user")
                { 
                    return RedirectToAction("UserDashboard", "User");
                }
                else if (existingUser.Role == "admin")
                {
                    return RedirectToAction("AdminDashboard", "Admin");
                }
            }

            ViewBag.Error = "Incorrect Credentials";
            return View();
        }




        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel user)
        {

            Users? oldUser =  await _userService.GetUserByEmailAsync(user.Email);
            if (oldUser != null)
            {
                ViewBag.Error = "Email Already exist";
                return View(user);
            }


            Random r = new Random();
            int otp = r.Next(100000, 999999);

            TempData["otp"] = otp.ToString();
            TempData["email"] = user.Email;
            TempData["name"] = user.FullName;
            TempData["password"] = user.Password;


            SendOTP(user.Email, otp.ToString());



            return RedirectToAction("VerifyEmail");
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            EmailVerifyViewModel m = new EmailVerifyViewModel();
            m.Email = TempData["email"]?.ToString();
            TempData.Keep(); 
            return View(m);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(EmailVerifyViewModel model)
        {
            string? otpStored = TempData["otp"]?.ToString();
            TempData.Keep();

            if (model.OtpCode == otpStored)
            {
                TempData["msg"] = "Email verified successfully!";
                string? name = TempData["name"]?.ToString();
                string? email = TempData["email"]?.ToString();
                string? password = TempData["password"]?.ToString();

                if (string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "Registration data expired. Please register again.";
                    return RedirectToAction("Register");
                }


                await _userService.CreateUserAsync(new Users
                {
                    FullName = name,
                    Email = email ?? string.Empty,
                    Password = password
                });


                return RedirectToAction("Login");
            }

            ViewBag.Error = "Invalid OTP. Try again.";
            return View(model);
        }

        public void SendOTP(string email, string otpCode)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential("tripvista53@gmail.com", "kpbh nghp xytf earj")
            };

            var msg = new MailMessage("tripvista53@gmail.com", email);
            msg.Subject = "Your SmartTrip Email Verification Code";
            msg.Body = $"Your OTP Code is: {otpCode}";

            client.Send(msg);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }


    }
}
