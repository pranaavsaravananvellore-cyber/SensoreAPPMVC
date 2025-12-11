using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreAPPMVC.Data;
using SensoreAPPMVC.Models;
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

        [RoleCheck("Clinician")]
        public IActionResult Dashboard()
        {
            var clinicianId = HttpContext.Session.GetInt32("UserId");

            var patients = _context.Patients
                .Where(p => p.ClinicianId == clinicianId)
                .OrderBy(p => p.Name)
                .ToList();

            return View(patients);
        }

        [RoleCheck("Clinician")]
        public IActionResult Heatmaps(int patientId)
        {
            var maps = _context.PressureMaps
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

            return View(maps);
        }

        [HttpPost]
        [RoleCheck("Clinician")]
        public async Task<IActionResult> SaveComment(int id, string comment)
        {
            var map = await _context.PressureMaps.FindAsync(id);

            if (map == null)
                return NotFound();

            map.ClinicianComment = comment;
            await _context.SaveChangesAsync();

            return RedirectToAction("Heatmaps", new { patientId = map.PatientId });
        }
    }
}
