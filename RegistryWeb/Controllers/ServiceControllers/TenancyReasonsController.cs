using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class TenancyReasonsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public TenancyReasonsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteReason(int? idReason)
        {
            if (idReason == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return -2;
            try
            {
                var reason = registryContext.TenancyReasons
                    .FirstOrDefault(op => op.IdReason == idReason);
                reason.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetReason(int? idReason)
        {
            if (idReason == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return Json(-2);
            var reason = registryContext.TenancyReasons
                .FirstOrDefault(op => op.IdReason == idReason);
            return Json(new {
                reasonNumber = reason.ReasonNumber,
                reasonDate = reason.ReasonDate.HasValue ? reason.ReasonDate.Value.ToString("yyyy-MM-dd") : null,
                idReasonType = reason.IdReasonType
            });
        }

        [HttpPost]
        public IActionResult SaveReason(TenancyReason tenancyReason)
        {
            if (tenancyReason == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(new { Error = -2 });

            var tenancyReasonType = registryContext.TenancyReasonTypes.FirstOrDefault(tr => tr.IdReasonType == tenancyReason.IdReasonType);
            if (tenancyReasonType == null)
                return Json(new { Error = -3 });
            tenancyReason.ReasonPrepared = tenancyReasonType.ReasonTemplate
                .Replace("@reason_date@", tenancyReason.ReasonDate.HasValue ? tenancyReason.ReasonDate.Value.ToString("dd.MM.yyyy") : "")
                .Replace("@reason_number@", tenancyReason.ReasonNumber);
            //Создать
            if (tenancyReason.IdReason == 0)
            {
                registryContext.TenancyReasons.Add(tenancyReason);
                registryContext.SaveChanges();

                return Json(new { tenancyReason.IdReason });
            }
            //Обновить            
            registryContext.TenancyReasons.Update(tenancyReason);
            registryContext.SaveChanges();
            return Json(new { tenancyReason.IdReason });
        }
    }
}