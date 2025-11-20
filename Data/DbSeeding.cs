using System.Linq;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Utilities;
using System.Threading.Tasks;

namespace SensoreAPPMVC.Data
{
    public static class DbSeeding
    {
        public static async Task dBInitalisation(AppDBContext context)
        {

            await context.Database.EnsureCreatedAsync();
            //check if admin user exists
            if (context.Users.Any())
            {
                return; //DB has already been seeded
            }

            //create default admin user
            string adminPasswordRaw = "12345"; //default password
            string hashedPassword = PasswordHasher.HashPassword(adminPasswordRaw);
            //   //TODO: hash password when hashing class is made
            var adminUser = new User()
            {
                Email = "Admin@aru.com",
                HashedPassword = hashedPassword, //TODO: set hashed password
                Name = "Admin User",
                DOB = DateOnly.Parse("16-12-2003"),
                Role = "Admin"
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        } 
    }  
}