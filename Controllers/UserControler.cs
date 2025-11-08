using Microsoft.AspNetCore.Mvc;
using SensoreAPPMVC.Models;
namespace SensoreAPPMVC.Controllers
{
    public class UserControler : Controller{

        public IActionResult Login()
        {
            return Login();
        }


    }

}