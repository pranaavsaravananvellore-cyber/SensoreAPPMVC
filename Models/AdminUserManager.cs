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

        public User CreateUser(string Email, string password, string role, string name, DateOnly dob, int clinicianId = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(Email))
                throw new ArgumentException("Email is required", nameof(Email));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required", nameof(password));
            if (string.IsNullOrWhiteSpace(role))
                role = "patient";

            var existing = _context.Users.SingleOrDefault(u => u.Email == Email);
            if (existing != null)
                throw new InvalidOperationException("A user with this email already exists.");

            var hashed = PasswordHasher.HashPassword(password);

            User user;

            if (role.Equals("patient", StringComparison.OrdinalIgnoreCase))
            {
                user = new Patient
                {
                    Name = name,
                    Email = Email,
                    HashedPassword = hashed,
                    Role = "Patient", // note capitalisation – match PatientController check
                    DOB = dob,
                    ClinicianId = clinicianId,
                    CompletedRegistration = clinicianId != 0
                };
            }
            else
            {
                user = new User
                {
                    Name = name,
                    Email = Email,
                    HashedPassword = hashed,
                    Role = role,
                    DOB = dob
                };
            }

            _context.Users.Add(user);    // one table, TPH handles Patient/User
            _context.SaveChanges();
            return user;
        }
    }
}