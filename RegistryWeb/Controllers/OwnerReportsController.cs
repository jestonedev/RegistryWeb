using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ReportServices;
using RegistryWeb.ViewModel;
using RegistryWeb.DataServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerReportsController : RegistryBaseController
    {
        private readonly ReportService reportService;
        private readonly OwnerReportsDataService dataService;
        private readonly SecurityService securityService;

        public OwnerReportsController(ReportService reportService, OwnerReportsDataService dataService, SecurityService securityService)
        {
            this.reportService = reportService;
            this.dataService = dataService;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View();
        }

        public IActionResult ReestrEmergencyPremises()
        {
            return RedirectToAction("Index", "ReestrEmergencyPremises");
        }

        public IActionResult Forma1()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View("~/Views/OwnerReports/Forma1/Index.cshtml", dataService.GetForma1VM());
        }

        public IActionResult GetForma1(int? idBuilding)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (idBuilding == null)
                return Error("id здания не указан.");
            var ids = new List<int> { idBuilding.Value };
            try
            {
                var file = reportService.Forma1(ids);
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Форма 1. Общие сведения об аварийном многоквартирном доме г. Братск.docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult Forma2()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View("~/Views/OwnerReports/Forma2/Index.cshtml", dataService.GetForma2VM());
        }

        public IActionResult GetForma2(int? idPremise)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (idPremise == null)
                return Error("id здания не указан.");
            //var ids = new List<int> { idPremise.Value };
            try
            {
                var file = reportService.Forma2(idPremise.Value);
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Форма 2. Сведения о жилых помещениях и собственниках (нанимателях) жилых помещений аварийного многоквартирного дома.docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}