using System;
using System.Collections.Generic;

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
        public string? GridData { get; set; }  // Make nullable
        
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}