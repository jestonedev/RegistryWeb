using System.Diagnostics;
using RegistryWeb.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using System.Linq;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    public class HomeController : Controller
    {
        private SecurityService securityService;
        public HomeController(SecurityService securityService)
        {
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
