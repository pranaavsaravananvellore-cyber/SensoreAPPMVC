using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            //plainText password check
            if (user.Password == model.Password)
            {
                //redirecting to appropirate dashboard based on user role
                switch(user.Role)
                {
                    case "Admin":
                        return RedirectToAction("Dashboard", "Admin");
                    case "Clinition":
                        return RedirectToAction("Dashboard", "Clinition");
                    case "patient":
                        return RedirectToAction("Dashboard", "Patient");
                    default:
                        ModelState.AddModelError(string.Empty, "Invalid user role.");
                        return View(model);
                        
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        


    }
}