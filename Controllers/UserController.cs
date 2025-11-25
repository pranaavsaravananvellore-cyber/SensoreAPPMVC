using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Models;
namespace SensoreAPPMVC.Controllers
{
    public class UserController : Controller{

        public IActionResult Login()
        {
            return Login();
        }


    }

}