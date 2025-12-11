using Microsoft.AspNetCore.Identity;

namespace SensoreAPPMVC.Models;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateOnly DOB { get; set; }
    
    // Password reset fields
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    
    // Security questions
    public string? SecurityQuestion1 { get; set; }
    public string? SecurityAnswer1 { get; set; }
    public string? SecurityQuestion2 { get; set; }
    public string? SecurityAnswer2 { get; set; }
}