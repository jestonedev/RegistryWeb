using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace RegistryWeb.Controllers
{
    public class RegistryBaseController : Controller
    {
        public string Name { get; private set; }

        public RegistryBaseController()
        {
            var name = GetType().Name;
            var position = name.IndexOf("Controller");
            Name = name.Substring(0, position);
        }

        public IActionResult Error(string msg = "")
        {
            ViewData["TextError"] = new HtmlString(msg);
            ViewData["Controller"] = Name;
            return View("Error");
        }
    }
}
