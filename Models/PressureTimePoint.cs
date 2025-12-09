using System;

namespace SensoreAPPMVC.Models
{
    /// <summary>
    /// Represents a single analysed pressure data point for a patient
    /// over a given time window (for example one recording file or session).
    /// </summary>
    public class PressureTimePoint
    {
        public DateTime Timestamp { get; set; }
        public int PatientId { get; set; }

        /// <summary>
        /// Maximum pressure value observed in the map for this time point.
        /// </summary>
        public double PeakPressure { get; set; }

        /// <summary>
        /// Percentage of sensor cells that are active (non-zero) over the map.
        /// </summary>
        public double ContactAreaPercent { get; set; }

        /// <summary>
        /// True if this time point exceeds the configured high-pressure threshold.
        /// </summary>
        public bool IsHighRisk { get; set; }
    }
}
