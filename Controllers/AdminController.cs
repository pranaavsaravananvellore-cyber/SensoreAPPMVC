using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Data;

namespace SensoreAPPMVC.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly AppDBContext _context;
        public AdminController(AppDBContext context)
        {
            _context = context;
        }
        
        [Route("[controller]/[action]")]
        public IActionResult Dashboard()
        {
            
            //validating admin access
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            Console.WriteLine($"Session UserId: {userId}");
            Console.WriteLine($"Session UserRole: {userRole}");
            


            if (userId == null || userRole != "Admin")
            {
                
                return RedirectToAction("Login", "Account");
            }
            
            
            // Fetch necessary data for the admin dashboard
             var userName = HttpContext.Session.GetString("UserName");
            ViewBag.UserName = userName;

            // Get admin dashboard data
            

            //send view 
            
            return View();
        }
    }
}