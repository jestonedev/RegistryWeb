using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace RegistryWeb.ViewComponents
{
    public class DatabaseNameComponent : ViewComponent
    {
        private IConfiguration config;

        public DatabaseNameComponent(IConfiguration config)
        {
            this.config = config;
        }

        public IViewComponentResult Invoke()
        {
            ViewBag.Database = config.GetValue<string>("Database");
            return View("DatabaseName");
        }
    }
}