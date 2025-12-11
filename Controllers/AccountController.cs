using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SensoreAPPMVC.Utilities;
using SensoreAPPMVC.Services;

namespace SensoreAPPMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDBContext _context;
        private readonly AdminServices _adminServices;

        public AccountController(AppDBContext context, AdminServices adminServices)
        {
            _context = context;
            _adminServices = adminServices;
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
                        
                    case "Clinician":
                        return RedirectToAction("Dashboard", "Clinician");
                    case "Patient":
                        return RedirectToAction("Dashboard", "Patient", new { id = user.UserId });
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

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPasswordEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user != null)
            {
                // Create a reset request for admin
                var resetRequest = await _adminServices.CreatePasswordResetRequestAsync(user.UserId, "Email");
                
                // Notify admins (you can send email here)
                await NotifyAdminsOfResetRequest(user, resetRequest);
            }

            // Don't reveal if email exists
            ViewBag.Message = "If an account exists with this email, administrators have been notified. They will contact you shortly.";
            return View("ForgotPassword");
        }

        [HttpGet]
        public async Task<IActionResult> ForgotPasswordSecurityQuestions(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null || string.IsNullOrEmpty(user.SecurityQuestion1))
            {
                ViewBag.Error = "This account does not have security questions set up or email not found.";
                return View();
            }

            var model = new SecurityQuestionsViewModel
            {
                Email = email,
                Question1 = user.SecurityQuestion1,
                Question2 = user.SecurityQuestion2
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifySecurityAnswers(string email, string answer1, string answer2)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null || string.IsNullOrEmpty(user.SecurityAnswer1))
            {
                ViewBag.Error = "Account not found or security questions not set up.";
                return View("ForgotPasswordSecurityQuestions");
            }

            // Verify answers
            bool answer1Valid = PasswordHasher.VerifyPassword(answer1.ToLower().Trim(), user.SecurityAnswer1);
            bool answer2Valid = PasswordHasher.VerifyPassword(answer2.ToLower().Trim(), user.SecurityAnswer2);

            if (!answer1Valid || !answer2Valid)
            {
                ViewBag.Error = "Incorrect answers. Please try again.";
                var model = new SecurityQuestionsViewModel
                {
                    Email = email,
                    Question1 = user.SecurityQuestion1,
                    Question2 = user.SecurityQuestion2
                };
                return View("ForgotPasswordSecurityQuestions", model);
            }

            // Create reset request for admin notification
            var resetRequest = await _adminServices.CreatePasswordResetRequestAsync(user.UserId, "SecurityQuestions");
            
            // Generate a temporary token for password reset
            var resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            return RedirectToAction("ResetPassword", new { token = resetToken });
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                ViewBag.Error = "Invalid or expired reset link.";
                return View();
            }

            return View(new ResetPasswordRequest { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(request);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.ResetToken == request.Token && u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                ViewBag.Error = "Invalid or expired reset link.";
                return View(request);
            }

            user.HashedPassword = PasswordHasher.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();

            ViewBag.Success = "Your password has been reset successfully. You can now login.";
            return RedirectToAction("Login");
        }

        private async Task NotifyAdminsOfResetRequest(User user, PasswordResetRequest resetRequest)
        {
            var admins = await _context.Users.Where(u => u.Role == "Admin").ToListAsync();
            
            foreach (var admin in admins)
            {
                // TODO: Send email notification to admin about reset request
                // For now, just log it
                System.Diagnostics.Debug.WriteLine($"[ADMIN ALERT] Password reset requested for user: {user.Name} ({user.Email})");
            }
        }
    }
}