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
            int? clinicianId = null)
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

                if (clinicianId.HasValue && clinicianId.Value > 0)
                {
                    // Pre-assigned patient
                    patient.ClinicianId = clinicianId.Value;
                    patient.CompletedRegistration = true;
                }
                else
                {
                    // Unassigned patient
                    patient.ClinicianId = null;
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
            int? clinicianId = null)
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
                if (clinicianId.HasValue && clinicianId.Value > 0)
                {
                    patient.ClinicianId = clinicianId.Value;
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

        public async Task<bool> UpdateSecurityQuestionsAsync(
            int userId,
            string question1,
            string answer1,
            string question2,
            string answer2)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            user.SecurityQuestion1 = question1;
            user.SecurityAnswer1 = PasswordHasher.HashPassword(answer1.ToLower().Trim());
            user.SecurityQuestion2 = question2;
            user.SecurityAnswer2 = PasswordHasher.HashPassword(answer2.ToLower().Trim());

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PasswordResetRequest> CreatePasswordResetRequestAsync(int userId, string requestType)
        {
            var request = new PasswordResetRequest
            {
                UserId = userId,
                RequestType = requestType,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetRequests.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<List<PasswordResetRequest>> GetPendingResetRequestsAsync()
        {
            return await _context.PasswordResetRequests
                .Where(r => r.Status == "Pending")
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ApproveResetRequestAsync(int requestId, string newPassword)
        {
            var resetRequest = await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (resetRequest == null)
                return false;

            var user = await GetUserByIdAsync(resetRequest.UserId);
            if (user == null)
                return false;

            user.HashedPassword = PasswordHasher.HashPassword(newPassword);
            resetRequest.Status = "Approved";
            resetRequest.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectResetRequestAsync(int requestId, string? notes = null)
        {
            var resetRequest = await _context.PasswordResetRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (resetRequest == null)
                return false;

            resetRequest.Status = "Rejected";
            resetRequest.ResolvedAt = DateTime.UtcNow;
            resetRequest.Notes = notes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteResetRequestAsync(int requestId)
        {
            var request = await _context.PasswordResetRequests.FindAsync(requestId);
            if (request == null)
                return false;

            _context.PasswordResetRequests.Remove(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PasswordResetRequest?> GetResetRequestByIdAsync(int requestId)
        {
            return await _context.PasswordResetRequests.FindAsync(requestId);
        }

        public async Task<bool> DismissResetRequestAsync(int requestId)
        {
            var resetRequest = await _context.PasswordResetRequests.FindAsync(requestId);
            if (resetRequest == null)
                return false;

            resetRequest.Status = "Dismissed";
            resetRequest.ResolvedAt = DateTime.UtcNow;
            
            _context.Entry(resetRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return true;
        }
    }
}