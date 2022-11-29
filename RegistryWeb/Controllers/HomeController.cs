using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class HomeController : RegistryBaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult UnsupportedBrowser()
        {
            return View("UnsupportedBrowser");
        }
    }
}
