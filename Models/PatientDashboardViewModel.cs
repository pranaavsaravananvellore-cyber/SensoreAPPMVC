using System.Collections.Generic;

namespace SensoreAPPMVC.Models
{
    public class PatientDashboardViewModel
    {
        public int PatientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Latest metrics
        public double CurrentPeakPressure { get; set; }
        public double CurrentContactAreaPercent { get; set; }
        public bool CurrentIsHighRisk { get; set; }

        // Simple comparison with previous period
        public double PreviousAvgPeakPressure { get; set; }
        public double PreviousAvgContactAreaPercent { get; set; }
        public int PreviousHighRiskEventCount { get; set; }

        public double CurrentAvgPeakPressure { get; set; }
        public double CurrentAvgContactAreaPercent { get; set; }
        public int CurrentHighRiskEventCount { get; set; }

        /// <summary>
        /// All analysed pressure points for the selected period, in chronological order.
        /// </summary>
        public List<PressureTimePoint> History { get; set; } = new List<PressureTimePoint>();

        /// <summary>
        /// Human-readable label for the currently selected period (e.g. "Last 7 days").
        /// </summary>
        public string SelectedPeriodLabel { get; set; } = "All data";
    }
}
