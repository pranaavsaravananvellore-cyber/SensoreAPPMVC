namespace GraphineTraceProgramRider.Models;

public class User
{
    public int UserId { get; set;}
    
    
    public string name { get; set; }
    
    public DateOnly DOB { get; set; }
    
    public string email { get; set; }
    
    public string Role { get; set; }
    
    protected String Password { get; set; }
    
    
}