using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Data;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Utilities;

namespace SensoreAPPMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDBContext _context;

        public AdminController(AppDBContext context)
        {
            _context = context;
        }

        [Route("[controller]/[action]")]
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // For now load all users – you can refine later
            var users = _context.Users.ToList();
            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateAccount", new AdminCreateUserViewModel());
        }

        [HttpPost]
        public IActionResult Create(AdminCreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View("CreateAccount", model);

            var manager = new AdminUserManager(_context);

            // Create either User or Patient (handled in manager)
            manager.CreateUser(
                Email: model.Email,
                password: model.Password,
                role: model.Role,
                name: model.Name,
                dob: model.DOB,
                clinitionId: model.ClinitionId
            );

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
