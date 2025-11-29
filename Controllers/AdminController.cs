using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            // After creating the user, redirect to the dashboard (or adjust as needed)
            return RedirectToAction("Dashboard");
        }
        
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Route("Admin/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            var patient = user as Patient;

            var vm = new AdminEditUserViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                DOB = user.DOB,
                IsPatient = patient != null,
                CompletedRegistration = patient != null && patient.CompletedRegistration,
                ClinitionId = patient != null ? patient.ClinitionId : 0
            };

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null)
                return NotFound();

            user.Name = model.Name;
            user.Email = model.Email;
            user.Role = model.Role;
            user.DOB = model.DOB;

            if (user is Patient patient)
            {
                // Make sure we always give EF a non‑nullable int
                patient.CompletedRegistration = model.CompletedRegistration;
                patient.ClinitionId = (int)(model.ClinitionId);   // explicit cast
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}
