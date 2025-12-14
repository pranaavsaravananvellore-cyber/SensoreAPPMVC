using System;

namespace SensoreAPPMVC.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PressureMapId { get; set; }
        public int PatientId { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public PressureMap PressureMap { get; set; }
    }
}