using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryServices.ViewModel.Other;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PersonalSettingController : RegistryBaseController
    {
        private SecurityService securityService;
        private IConfiguration config;

        public PersonalSettingController(SecurityService securityService, IConfiguration configuration)
        {
            this.securityService = securityService;
            this.config = configuration;
        }

        [HttpGet]
        public IActionResult Details()
        {
            var personalSettingVM = new PersonalSettingVM();
            personalSettingVM.User = securityService.User;
            personalSettingVM.Privileges = securityService.Privileges;
            personalSettingVM.Database = config.GetValue<string>("Database");
            return View(personalSettingVM);
        }
    }
}