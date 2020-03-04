using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    public class PersonalSettingController : RegistryBaseController
    {
        private RegistryContext registryContext;
        private SecurityService securityService;
        public PersonalSettingController(RegistryContext registryContext, SecurityService securityService)
        {
            this.registryContext = registryContext;
            this.securityService = securityService;
        }

        [HttpGet]
        public IActionResult Details()
        {
            var personalSetting = registryContext.PersonalSettings
                .Include(ps => ps.IdUserNavigation)
                .AsNoTracking()
                .SingleOrDefault(ps => ps.IdUserNavigation == securityService.User);
            return View(personalSetting);
        }

        [HttpPost]
        public IActionResult Save(PersonalSetting personalSetting)
        {
            registryContext.Entry(personalSetting).State = EntityState.Unchanged;
            registryContext.Entry(personalSetting).Property(ps => ps.SqlDriver).IsModified = true;
            registryContext.Entry(personalSetting.IdUserNavigation).State = EntityState.Unchanged;
            registryContext.Entry(personalSetting.IdUserNavigation).Property(user => user.UserDescription).IsModified = true;
            registryContext.Update(personalSetting);
            registryContext.SaveChanges();
            return View("Details", personalSetting);
        }
    }
}