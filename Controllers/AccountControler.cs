using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Data;
using System.Linq;



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
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Here you would typically validate the user credentials against the database
                /*
                var user = _context.Users
                    .FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);

                if (user != null)
                {
                    // User is authenticated, redirect to a secure area
                    return RedirectToAction("Index", "Home");
                }
                */
                if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password))
                {
                    // For demonstration purposes, we assume any non-empty credentials are valid
                    return RedirectToAction("Index", "Home");   
                }
    

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }


    }
}