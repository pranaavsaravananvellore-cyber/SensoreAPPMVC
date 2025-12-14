using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Services;

namespace SensoreAPPMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminServices _adminServices;

        public AdminController(AdminServices adminServices)
        {
            _adminServices = adminServices;
        }

        [Route("[controller]/[action]")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            // Use service instead of _context
            var users = await _adminServices.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Admin")
                return RedirectToAction("Login", "Account");

            var model = new AdminCreateUserViewModel();

            // Load clinicians for dropdown
            var clinicians = await _adminServices.GetAllCliniciansAsync();
            model.ClinicianList = clinicians.Select(c => new SelectListItem
            {
                Value = c.UserId.ToString(),  // This is the ID (what gets sent to server)
                Text = c.Name                  // This is what the user sees
            }).ToList();

            return View("CreateAccount", model);
        }

        [HttpPost]
    public async Task<IActionResult> Create(AdminCreateUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload clinicians if validation fails
            var clinicians = await _adminServices.GetAllCliniciansAsync();
            model.ClinicianList = clinicians.Select(c => new SelectListItem
            {
                Value = c.UserId.ToString(),
                Text = c.Name
            }).ToList();
            
            return View("CreateAccount", model);
        }

        var success = await _adminServices.CreateUserAsync(
            model.Name,
            model.Email,
            model.Password,
            model.Role,
            model.DOB,
            model.ClinicianId
        );

        if (!success)
        {
            ModelState.AddModelError("Email", "Email already exists!");
            
            // Reload clinicians
            var clinicians = await _adminServices.GetAllCliniciansAsync();
            model.ClinicianList = clinicians.Select(c => new SelectListItem
            {
                Value = c.UserId.ToString(),
                Text = c.Name
            }).ToList();
            
            return View("CreateAccount", model);
        }

        TempData["Success"] = "User created successfully!";
        return RedirectToAction("Dashboard");
    }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Route("Admin/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Admin")
                return RedirectToAction("Login", "Account");

            var user = await _adminServices.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("Dashboard");
            }

            var patient = user as Patient;

            var vm = new AdminEditUserViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                DOB = user.DOB,
                IsPatient = patient != null,
                CompletedRegistration = patient?.CompletedRegistration ?? false,
                ClinicianId = patient?.ClinicianId ?? 0
            };

            // Load clinicians for dropdown
            var clinicians = await _adminServices.GetAllCliniciansAsync();
            vm.ClinicianList = clinicians.Select(c => new SelectListItem
            {
                Value = c.UserId.ToString(),
                Text = c.Name
            }).ToList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminEditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload clinicians if validation fails
                var clinicians = await _adminServices.GetAllCliniciansAsync();
                model.ClinicianList = clinicians.Select(c => new SelectListItem
                {
                    Value = c.UserId.ToString(),
                    Text = c.Name
                }).ToList();
                
                return View(model);
            }

            var success = await _adminServices.UpdateUserAsync(
                model.UserId,
                model.Name,
                model.Email,
                model.Role,
                model.DOB,
                completedRegistration: null,    // no manual slider
                clinicianId: model.ClinicianId  // clinician drives completion
            );

            if (!success)
            {
                ModelState.AddModelError("", "Update failed. Email may already be in use.");
                
                var clinicians = await _adminServices.GetAllCliniciansAsync();
                model.ClinicianList = clinicians.Select(c => new SelectListItem
                {
                    Value = c.UserId.ToString(),
                    Text = c.Name
                }).ToList();
                
                return View(model);
            }

            TempData["Success"] = "User updated successfully!";
            return RedirectToAction("Dashboard");
        }

            [HttpPost]
            public async Task<IActionResult> Delete(int id)
            {
                var success = await _adminServices.DeleteUserAsync(id);

                if (success)
                {
                    TempData["Success"] = "User deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete user";
                }

                return RedirectToAction("Dashboard");
            }

        [HttpGet]
        public async Task<IActionResult> GetPendingAlerts()
        {
            var alerts = await _adminServices.GetPendingResetRequestsAsync();
            
            var alertList = alerts.Select(a => new
            {
                id = a.Id,
                userId = a.UserId,
                userName = a.User?.Name ?? "Unknown",
                userEmail = a.User?.Email ?? "Unknown",
                requestType = a.RequestType,
                createdAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                message = $"{a.User?.Name} ({a.RequestType}) requested password reset",
                editUrl = $"/Admin/Edit/{a.UserId}"
            }).ToList();
            
            return Json(alertList);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveReset([FromBody] dynamic request)
        {
            int requestId = request.requestId;
            string newPassword = request.newPassword;
            
            var success = await _adminServices.ApproveResetRequestAsync(requestId, newPassword);
            return Json(new { success = success });
        }

        [HttpPost]
        public async Task<IActionResult> RejectReset([FromBody] dynamic request)
        {
            int requestId = request.requestId;
            string notes = request.notes;
            
            var success = await _adminServices.RejectResetRequestAsync(requestId, notes);
            return Json(new { success = success });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DismissAlert(int requestId)
        {
            var success = await _adminServices.DismissResetRequestAsync(requestId);
            return Json(new { success = success });
        }
    }
}