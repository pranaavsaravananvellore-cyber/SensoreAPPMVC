using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreAPPMVC.Data;
using SensoreAPPMVC.Models;

namespace SensoreAPPMVC.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppDBContext _context;

        public PatientController(AppDBContext context)
        {
            _context = context;
        }

        // GET: /Patient/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userIdObj = HttpContext.Session.GetInt32("UserId");
            if (userIdObj == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = userIdObj.Value;

            var patient = await _context.Users
                .OfType<Patient>()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            var vm = new PatientDashboardViewModel
            {
                PatientId = patient.UserId,
                Name = patient.Name,
                Email = patient.Email
            };

            return View("PatientDashboard", vm);
        }
    }
}