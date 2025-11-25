using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Data;
using Microsoft.AspNetCore;

namespace SensoreAPPMVC.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppDBContext _context;
        public PatientController(AppDBContext context)
        {
            _context = context;
        }
        [Route("[controller]/[action]")]
        public async Task<IActionResult> Dashboard()
        {
            //validating patient access
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Patient")
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Fetch necessary data for the patient dashboard

            //send view
            return View();
        }
    }
}