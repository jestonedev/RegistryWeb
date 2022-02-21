using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryWeb.DataHelpers;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class ClaimPersonsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public ClaimPersonsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeletePerson(int? idPerson)
        {
            if (idPerson == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return -2;
            try
            {
                var person = registryContext.ClaimPersons
                    .FirstOrDefault(op => op.IdPerson == idPerson);
                person.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetPerson(int? idPerson)
        {
            if (idPerson == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return Json(-2);
            var person = registryContext.ClaimPersons
                .FirstOrDefault(op => op.IdPerson == idPerson);
            return Json(new {
                surname = person.Surname,
                name = person.Name,
                patronymic = person.Patronymic,
                dateOfBirth = person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("yyyy-MM-dd") : null,
                placeOfBirth = person.PlaceOfBirth,
                workPlace = person.WorkPlace,
                isClaimer = person.IsClaimer,
            });
        }

        [HttpPost]
        public IActionResult SavePerson(ClaimPerson claimPerson)
        {
            if (claimPerson == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Json(new { Error = -2 });
            //Создать
            if (claimPerson.IdPerson == 0)
            {
                registryContext.ClaimPersons.Add(claimPerson);
                registryContext.SaveChanges();

                return Json(new { claimPerson.IdPerson });
            }
            //Обновить            
            registryContext.ClaimPersons.Update(claimPerson);
            registryContext.SaveChanges();
            return Json(new { claimPerson.IdPerson });
        }
    }
}