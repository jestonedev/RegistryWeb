using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class KumiPaymentUfsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public KumiPaymentUfsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeletePaymentUf(int? idPaymentUf)
        {
            if (idPaymentUf == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.AccountsWrite))
                return -2;
            try
            {
                var paymentUf = registryContext.KumiPaymentUfs
                    .FirstOrDefault(op => op.IdPaymentUf == idPaymentUf);
                paymentUf.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult SavePaymentUf(KumiPaymentUf kumiPaymentUf)
        {
            if (kumiPaymentUf == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.AccountsWrite))
                return Json(new { Error = -2 });

            //Создать
            if (kumiPaymentUf.IdPaymentUf == 0)
            {
                registryContext.KumiPaymentUfs.Add(kumiPaymentUf);
                registryContext.SaveChanges();
                return Json(new { kumiPaymentUf.IdPaymentUf });
            }
            //Обновить            
            registryContext.KumiPaymentUfs.Update(kumiPaymentUf);
            registryContext.SaveChanges();
            return Json(new { kumiPaymentUf.IdPaymentUf });
        }
    }
}