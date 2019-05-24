using System.Diagnostics;
using RegistryWeb.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using System.Linq;

namespace RegistryWeb.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
