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
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using NameCaseLib;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class TenancyAgreementsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public TenancyAgreementsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteAgreement(int? idAgreement)
        {
            if (idAgreement == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return -2;
            try
            {
                var agreement = registryContext.TenancyAgreements
                    .FirstOrDefault(op => op.IdAgreement == idAgreement);
                agreement.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult SaveAgreement(TenancyAgreement agreement)
        {
            if (agreement == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(new { Error = -2 });

            //Создать
            if (agreement.IdAgreement == 0)
            {
                registryContext.TenancyAgreements.Add(agreement);
                registryContext.SaveChanges();

                return Json(new { agreement.IdAgreement });
            }
            //Обновить            
            registryContext.TenancyAgreements.Update(agreement);
            registryContext.SaveChanges();
            return Json(new { agreement.IdAgreement });
        }

        [HttpGet]
        public IActionResult GetSnpPartsCase(string surname, string name, string patronymic, Padeg padeg)
        {
            return GetSnpCase((surname + " " + name + " " + patronymic).Trim(), padeg);
        }

        [HttpGet]
        public IActionResult GetSnpCase(string snp, Padeg padeg)
        {
            var ncl = new Ru();
            var snpAccusative = ncl.Q(snp, padeg);
            return Json(new { snpAccusative });
        }
    }
}