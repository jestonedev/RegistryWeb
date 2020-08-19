using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class BuildingReportsController : RegistryBaseController
    {
        private readonly BuildingReportService reportService;
        private readonly SecurityService securityService;
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";

        public BuildingReportsController(BuildingReportService reportService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.securityService = securityService;
        }

        public IActionResult GetExcerptBuilding(int idBuilding, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.ExcerptBuilding(idBuilding, excerptNumber, excerptDateFrom, signer);
                return File(file, odsMime, string.Format(@"Выписка на здание № {0}", idBuilding));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}