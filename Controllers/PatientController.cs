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

        public PatientController(AppDBContext context)
        {
            _context = context;
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
            // Get logged-in user from session
            var sessionUserId = HttpContext.Session.GetInt32("UserId");

            // AUTHORIZATION CHECK: Patients can only view their own data
            if (id != sessionUserId)
            {
                // Redirect to their own dashboard
                return RedirectToAction("Dashboard", "Patient", new { id = sessionUserId });
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            // Resolve date range from the requested period.
            DateTime? from = null;
            DateTime? to = null;
            var now = DateTime.UtcNow.Date;
            string periodLabel;

            switch (period?.ToLowerInvariant())
            {
                case "7d":
                    from = now.AddDays(-6);
                    to = now;
                    periodLabel = "Last 7 days";
                    break;
                case "30d":
                    from = now.AddDays(-29);
                    to = now;
                    periodLabel = "Last 30 days";
                    break;
                default:
                    period = "all";
                    periodLabel = "All data";
                    break;
            }

            // Folder where pressure map CSV files are stored.
            // Expected naming convention: {patientId}_yyyyMMdd.csv
            var pressureRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pressuredata");
            var history = PressureDataAnalyzer.LoadPatientHistory(patient.UserId, pressureRoot, from, to);

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

            return View("PatientDashboard", vm);
        }
    }
}
