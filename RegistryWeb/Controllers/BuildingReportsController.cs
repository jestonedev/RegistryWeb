using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryServices.DataHelpers;
using RegistryWeb.Filters;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.RegistryRead)]
    [DefaultResponseOnException(typeof(Exception))]
    public class BuildingReportsController : RegistryBaseController
    {
        private readonly BuildingReportService reportService;
        private readonly SecurityService securityService;

        public BuildingReportsController(BuildingReportService reportService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.securityService = securityService;
        }

        public IActionResult GetExcerptBuilding(int idBuilding, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            var file = reportService.ExcerptBuilding(idBuilding, excerptNumber, excerptDateFrom, signer);
            return File(file, MimeTypeHelper.OdsMime, string.Format(@"Выписка на здание № {0}.odt", idBuilding));
        }
    }
}