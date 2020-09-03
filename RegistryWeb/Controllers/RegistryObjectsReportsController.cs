using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;
using RegistryWeb.ReportServices;
using RegistryWeb.DataServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class RegistryObjectsReportsController: RegistryBaseController
    {
        private readonly RegistryObjectsReportService reportService;
        private readonly RegistryObjectsDataService dataService;
        private readonly SecurityService securityService;
        private const string odtMime = "application/vnd.oasis.opendocument.text";
        private const string odsMime = "application/vnd.oasis.opendocument.spreadsheet";

        public RegistryObjectsReportsController(RegistryObjectsReportService reportService, RegistryObjectsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");

            var viewModel = dataService.GetViewModel();
            
            return View("Index", viewModel);
        }

        public IActionResult GetEmergencyJP(string JFType, string regions)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetJFReport(JFType, regions, out string NameReport);
                return File(file, odsMime, NameReport);
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetMunicipalBuilding(string typeReport)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetStatisticBuildingReport(typeReport);
                return File(file, odsMime, "Статистика по муниципальным жилым зданиям");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetMunicipalPremise(string typeReport)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            try
            {
                var file = reportService.GetStatisticPremiseReport(typeReport);
                return File(file, odsMime, "Статистика по муниципальным жилым помещениям");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }



    }
}