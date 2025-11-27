using Microsoft.EntityFrameworkCore.Storage;
using SensoreAPPMVC.Data;
using SensoreAPPMVC.Utilities;
namespace SensoreAPPMVC.Models
{
    public class AdminUserManager
    {
        private readonly AppDBContext _context;

        public AdminUserManager(AppDBContext context)
        {
            _context = context;
        }

        public int FindNewAccountId()
        {
            return _context.Users.Max(u => u.UserId) + 1;
        }

        public User CreateUser(string Email, string password, string role, string name, DateOnly dob)
        {

            var exsistingUser = _context.Users.SingleOrDefault(u => u.Email == Email);
            if (exsistingUser != null)
            {
                throw new Exception("User with this email already exsists.");
            }
            var user = new User
            {
                Email = Email,
                HashedPassword = PasswordHasher.HashPassword(password),
                Role = role,
                UserId = FindNewAccountId(),
                Name = name,
                DOB = dob
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }
    }
}