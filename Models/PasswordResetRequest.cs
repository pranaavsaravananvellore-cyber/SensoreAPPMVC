using System;

namespace SensoreAPPMVC.Models
{
    public class PasswordResetRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RequestType { get; set; } = string.Empty; // "Email" or "SecurityQuestions"
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int? ResolvedByAdminId { get; set; }
        public string? Notes { get; set; }
        public User? User { get; set; }
    }
}