using System;

namespace SensoreAPPMVC.Models
{
    public class PressureMap
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime Timestamp { get; set; }
        public double PeakPressure { get; set; }
        public double ContactAreaPercent { get; set; }
        public bool IsHighRisk { get; set; }
        public DateTime CreatedAt { get; set; }

        // NEW: Full 32×32 zero-masked grid stored as JSON
        public string? GridData { get; set; }

        // NEW: Clinician comment
        public string? ClinicianComment { get; set; }
    }
}
