using Microsoft.AspNetCore.Identity;

namespace SensoreAPPMVC.Models;

public class User
{
    //login credentials
    public string Email { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
    //public required string PasswordHash {get;set;}

    //general 
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;
    
    public DateOnly DOB { get; set; }

    public string Role { get; set; } = string.Empty;
}