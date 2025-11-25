using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SensoreAPPMVC.Utilities;

namespace SensoreAPPMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDBContext _context;

        public AccountController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == model.Email);
            //serching the user database by the email provided
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
            // password check
            if (PasswordHasher.VerifyPassword(model.Password, user.HashedPassword))
            {
                Console.WriteLine("PASSWORD VERIFIED");
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("UserRole", user.Role);
                Console.WriteLine($"Session Set - UserId: {user.UserId}, Name: {user.Name}, Role: {user.Role}");
                //redirecting to appropirate dashboard based on user role
                switch (user.Role)
                {
                    case "Admin":
                        return RedirectToAction("Dashboard", "Admin");
                        
                    case "Clinition":
                        return RedirectToAction("Dashboard", "Clinition");
                    case "Patient":
                        return RedirectToAction("Dashboard", "Patient");
                    default:

                        return RedirectToAction("Login", "Account");

                }
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}