using Microsoft.EntityFrameworkCore;
using SensoreAPPMVC.Data;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Utilities;

namespace SensoreAPPMVC.Services
{
    public class AdminServices
    {
        private readonly AppDBContext _context;

        public AdminServices(AppDBContext context)
        {
            _context = context;
        }

        // Get all users
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.OrderBy(u => u.Name).ToListAsync();
        }

        // Get user by ID
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
        }

        // Create user (handles both User and Patient)
        public async Task<bool> CreateUserAsync(
            string name,
            string email,
            string password,
            string role,
            DateOnly dob,
            int? clinitionId = null)
        {
            // Check if email already exists
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return false;

            var hashedPassword = PasswordHasher.HashPassword(password);

            User user;

            if (role == "Patient")
            {
                var patient = new Patient
                {
                    Name = name,
                    Email = email,
                    HashedPassword = hashedPassword,
                    Role = role,
                    DOB = dob
                };

                if (clinitionId.HasValue && clinitionId.Value > 0)
                {
                    // Pre-assigned patient
                    patient.ClinitionId = clinitionId.Value;
                    patient.CompletedRegistration = true;
                }
                else
                {
                    // Unassigned patient
                    patient.ClinitionId = null;
                    patient.CompletedRegistration = false;
                }

                user = patient;
            }
            else
            {
                user = new User
                {
                    Name = name,
                    Email = email,
                    HashedPassword = hashedPassword,
                    Role = role,
                    DOB = dob
                };
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Update user
        public async Task<bool> UpdateUserAsync(
            int userId,
            string name,
            string email,
            string role,
            DateOnly dob,
            bool? completedRegistration = null,
            int? clinitionId = null)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            // Check if email is taken by another user
            var emailTaken = await _context.Users
                .AnyAsync(u => u.Email == email && u.UserId != userId);
            if (emailTaken)
                return false;

            user.Name = name;
            user.Email = email;
            user.Role = role;
            user.DOB = dob;

            if (user is Patient patient)
            {
                // If a clinician is explicitly provided and > 0, assign & auto-complete registration
                if (clinitionId.HasValue && clinitionId.Value > 0)
                {
                    patient.ClinitionId = clinitionId.Value;
                    patient.CompletedRegistration = true;
                }
                else
                {
                    // No clinician passed – only then respect the completedRegistration flag
                    if (completedRegistration.HasValue)
                    {
                        patient.CompletedRegistration = completedRegistration.Value;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete user
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetAllCliniciansAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "Clinician")
                .OrderBy(u => u.Name)
                .ToListAsync();
        }
    }
}