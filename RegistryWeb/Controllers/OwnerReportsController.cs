
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerReportsController : Controller
    {
        private readonly SecurityService securityService;

        public OwnerReportsController(SecurityService securityService)
        {
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View();
        }
    }
}