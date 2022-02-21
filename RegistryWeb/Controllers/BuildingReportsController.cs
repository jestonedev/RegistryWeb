using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                return File(file, odtMime, string.Format(@"Выписка на здание № {0}.odt", idBuilding));
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}