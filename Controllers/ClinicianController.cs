using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Data;
using Microsoft.AspNetCore.Http;
using SensoreAPPMVC.Services;

namespace SensoreAPPMVC.Controllers
{
    public class ClinicianController : Controller
    {
        private readonly AppDBContext _context;
        public ClinicianController(AppDBContext context)
        {
            _context = context;
        }
        [Route("[controller]/[action]")]
        [RoleCheck("Clinician")]
        public async Task<IActionResult> Dashboard()
        {
            //validating cliniction access
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Clinician")
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch necessary data for the cliniction dashboard

            //send view
            return View();
        }
    }
}