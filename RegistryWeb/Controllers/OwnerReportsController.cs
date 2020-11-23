﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;
using RegistryWeb.ReportServices;
using RegistryWeb.DataServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerReportsController : RegistryBaseController
    {
        private readonly OwnerReportService reportService;
        private readonly OwnerReportsDataService dataService;
        private readonly SecurityService securityService;

        public OwnerReportsController(OwnerReportService reportService, OwnerReportsDataService dataService, SecurityService securityService)
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

        public IActionResult Forma1(Forma1VM forma1VM)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View("~/Views/OwnerReports/Forma1/Index.cshtml", dataService.GetForma1VM(forma1VM.FilterOptions));
        }

        public IActionResult GetForma1(int? idBuilding)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
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

        public IActionResult Forma2And3(Forma2VM forma2VM)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View("~/Views/OwnerReports/Forma2And3/Index.cshtml", dataService.GetForma2VM(forma2VM.FilterOptions));
        }

        public IActionResult GetForma2(int? idPremise)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            if (idPremise == null)
                return Error("Помещение не выбрано.");
            try
            {
                var file = reportService.Forma2(new List<int>() { idPremise.Value });
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Форма 2. Сведения о жилых помещениях и собственниках (нанимателях) жилых помещений аварийного многоквартирного дома.docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult MultiForma2(List<int> ids)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            if (!ids.Any())
                return Error("Не выбрано ни одного помещения. Либо в здании таковые отсутствуют.");
            try
            {
                var fileNameReport = reportService.Forma2Ajax(ids);
                return Json(new { fileNameReport });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetMultiForma2(string fileNameReport)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            try
            {
                var file = reportService.DownloadFile(fileNameReport);
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Форма 2. Сведения о жилых помещениях и собственниках (нанимателях) жилых помещений аварийного многоквартирного дома.docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetForma3(int? idPremise)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            if (idPremise == null)
                return Error("Помещение не выбрано.");
            try
            {
                var file = reportService.Forma3(new List<int>() { idPremise.Value });
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Форма 3. Сведения, необходимые для целей формирования программы переселения граждан  из аварийных многоквартирных домов.docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult MultiForma3(List<int> ids)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            if (!ids.Any())
                return Error("Не выбрано ни одного помещения. Либо в здании таковые отсутствуют.");
            try
            {
                var fileNameReport = reportService.Forma3Ajax(ids);
                return Json(new { fileNameReport });
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetMultiForma3(string fileNameReport)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            try
            {
                var file = reportService.DownloadFile(fileNameReport);
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Форма 3. Сведения, необходимые для целей формирования программы переселения граждан  из аварийных многоквартирных домов.docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetAzfAreaAnalize()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            try
            {
                var file = reportService.AzfAreaAnalize();
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    @"Анализ АЖФ на текущую дату (краткий) по площади.xlsx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetAzfRoomsAnalize()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            try
            {
                var file = reportService.AzfRoomsAnalize();
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    @"Анализ АЖФ на текущую дату (краткий) по количеству комнат.xlsx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetAzfRegionsAnalize()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            try
            {
                var file = reportService.AzfRegionsAnalize();
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    @"Анализ АЖФ на текущую дату (краткий) по жилым районам.xlsx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        public IActionResult GetAzfWithoutPrivAnalize()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            try
            {
                var file = reportService.AzfWithoutPrivAnalize();
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    @"Анализ АЖФ на текущую дату по МКД без частных ЖП.xlsx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }
    }
}