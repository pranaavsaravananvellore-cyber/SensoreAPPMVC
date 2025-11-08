namespace GraphineTraceProgramRider.Models;

public class User
{
    //login credentials
    public string Email { get; set; }
    protected String Password { get; set; }


    //general 
    public int UserId { get; set; }

    public string Name { get; set; }
    
    public DateOnly DOB { get; set; }

    public string Role { get; set; }
}