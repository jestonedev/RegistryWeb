using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers.ServiceControllers
{
    public class TenancyRentPeriodsController : Controller
    {
        SecurityService securityService;
        RegistryContext registryContext;
        private readonly IConfiguration config;

        public TenancyRentPeriodsController(SecurityService securityService, RegistryContext registryContext, IConfiguration config)
        {
            this.securityService = securityService;
            this.registryContext = registryContext;
            this.config = config;
        }

        [HttpPost]
        public int DeleteRentPeriod(int? idRentPeriod)
        {
            if (idRentPeriod == null)
                return -1;
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return -2;
            try
            {
                var rentPeriod = registryContext.TenancyRentPeriods
                    .FirstOrDefault(op => op.IdRentPeriod == idRentPeriod);
                rentPeriod.Deleted = 1;
                registryContext.SaveChanges();
                return 1;
            }
            catch
            {
                return -3;
            }
        }

        [HttpPost]
        public IActionResult GetRentPeriod(int? idRentPeriod)
        {
            if (idRentPeriod == null)
                return Json(-1);
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return Json(-2);
            var rentPeriod = registryContext.TenancyRentPeriods
                .FirstOrDefault(op => op.IdRentPeriod == idRentPeriod);
            return Json(new {
                beginDate = rentPeriod.BeginDate.HasValue ? rentPeriod.BeginDate.Value.ToString("yyyy-MM-dd") : null,
                endDate = rentPeriod.EndDate.HasValue ? rentPeriod.EndDate.Value.ToString("yyyy-MM-dd") : null,
                untilDismissal = rentPeriod.UntilDismissal
            });
        }

        [HttpPost]
        public IActionResult SaveRentPeriod(TenancyRentPeriod rentPeriod)
        {
            if (rentPeriod == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(new { Error = -2 });
            //Создать
            if (rentPeriod.IdRentPeriod == 0)
            {
                registryContext.TenancyRentPeriods.Add(rentPeriod);
                registryContext.SaveChanges();

                return Json(new { rentPeriod.IdRentPeriod });
            }
            //Обновить            
            registryContext.TenancyRentPeriods.Update(rentPeriod);
            registryContext.SaveChanges();
            return Json(new { rentPeriod.IdRentPeriod });
        }
    }
}