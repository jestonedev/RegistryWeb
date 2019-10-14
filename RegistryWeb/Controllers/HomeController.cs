using System.Diagnostics;
using RegistryWeb.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using System.Linq;
using RegistryWeb.SecurityServices;
using Microsoft.AspNetCore.Authorization;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
