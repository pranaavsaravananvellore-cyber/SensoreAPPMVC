using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SensoreAPPMVC.Data;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SensoreAPPMVC.Services;

namespace SensoreAPPMVC.Controllers
{
    public class PatientController : Controller
    {
        private readonly AppDBContext _context;
        private readonly UploadHandler _uploadHandler;

        public PatientController(AppDBContext context, UploadHandler uploadHandler)
        {
            _context = context;
            _uploadHandler = uploadHandler;
        }

        /// <summary>
        /// Displays the patient dashboard including analysed pressure data.
        /// </summary>
        /// <param name="id">Patient user identifier.</param>
        /// <param name="period">
        /// Optional period selector: "7d", "30d" or "all".
        /// This controls how much historical pressure data is loaded.
        /// </param>
        /// 
        [RoleCheck("Patient")]
        public async Task<IActionResult> Dashboard(int id, string? period = "7d")
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (id != sessionUserId)
            {
                return RedirectToAction("Dashboard", "Patient", new { id = sessionUserId });
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            DateTime? from = null;
            DateTime? to = null;
            var now = DateTime.UtcNow.Date;
            string periodLabel;

            switch (period?.ToLowerInvariant())
            {
                case "7d":
                    from = now.AddDays(-6);
                    to = now.AddDays(1);
                    periodLabel = "Last 7 days";
                    break;
                case "30d":
                    from = now.AddDays(-29);
                    to = now.AddDays(1);
                    periodLabel = "Last 30 days";
                    break;
                default:
                    period = "all";
                    periodLabel = "All data";
                    break;
            }

            // Load from DATABASE instead of file system
            var history = PressureDataAnalyzer.LoadPatientHistoryFromDatabase(_context, patient.UserId, from, to);

            var vm = new PatientDashboardViewModel
            {
                PatientId = patient.UserId,
                Name = patient.Name,
                Email = patient.Email,
                History = history,
                SelectedPeriodLabel = periodLabel
            };

            if (history.Any())
            {
                var latest = history.OrderBy(h => h.Timestamp).Last();
                vm.CurrentPeakPressure = latest.PeakPressure;
                vm.CurrentContactAreaPercent = latest.ContactAreaPercent;
                vm.CurrentIsHighRisk = latest.IsHighRisk;

                var (current, previous) = PressureDataAnalyzer.SplitCurrentAndPrevious(history);
                var (curAvgPeak, curAvgContact, curHighRisk) = PressureDataAnalyzer.Summarise(current);
                var (prevAvgPeak, prevAvgContact, prevHighRisk) = PressureDataAnalyzer.Summarise(previous);

                vm.CurrentAvgPeakPressure = curAvgPeak;
                vm.CurrentAvgContactAreaPercent = curAvgContact;
                vm.CurrentHighRiskEventCount = curHighRisk;

                vm.PreviousAvgPeakPressure = prevAvgPeak;
                vm.PreviousAvgContactAreaPercent = prevAvgContact;
                vm.PreviousHighRiskEventCount = prevHighRisk;
            }

            ViewBag.SelectedPeriod = period ?? "7d";
            return View("PatientDashboard", vm);
        }

        [Route("Patient/UploadFile")]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var patientId = HttpContext.Session.GetInt32("UserId");
            if (patientId == null) return Unauthorized();

            var (success, message, recordCount) = await _uploadHandler.Upload(file, patientId.Value);
            return success 
                ? Ok(new { success = true, message = message, recordCount = recordCount })
                : BadRequest(new { success = false, message = message });
        }

        [Route("Patient/ClearData")]
        [HttpPost]
        public async Task<IActionResult> ClearData()
        {
            var patientId = HttpContext.Session.GetInt32("UserId");
            if (patientId == null) return Unauthorized();

            var records = _context.PressureMaps.Where(p => p.PatientId == patientId);
            var count = records.Count();
            _context.PressureMaps.RemoveRange(records);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, recordsDeleted = count });
        }
    }
}
