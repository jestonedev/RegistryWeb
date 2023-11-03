using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.SecurityServices;
using RegistryWeb.ReportServices;
using RegistryWeb.DataServices;
using RegistryServices.ViewModel.Owners;
using RegistryWeb.Filters;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.OwnerRead)]
    [DefaultResponseOnException(typeof(Exception))]
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
            return View();
        }

        public IActionResult ReestrEmergencyPremises()
        {
            return RedirectToAction("Index", "ReestrEmergencyPremises");
        }

        public IActionResult Forma1(Forma1VM forma1VM)
        {
            return View("~/Views/OwnerReports/Forma1/Index.cshtml", dataService.GetForma1VM(forma1VM.FilterOptions));
        }

        public IActionResult GetForma1(int? idBuilding)
        {
            if (idBuilding == null)
                return Error("id здания не указан.");
            var ids = new List<int> { idBuilding.Value };
            var file = reportService.Forma1(ids);
            return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                @"Форма 1. Общие сведения об аварийном многоквартирном доме г. Братск.docx");
        }

        public IActionResult Forma2And3(Forma2VM forma2VM)
        {
            return View("~/Views/OwnerReports/Forma2And3/Index.cshtml", dataService.GetForma2VM(forma2VM.FilterOptions));
        }

        public IActionResult GetForma2(int? idPremise)
        {
            if (idPremise == null)
                return Error("Помещение не выбрано.");
            var file = reportService.Forma2(new List<int>() { idPremise.Value });
            return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                @"Форма 2. Сведения о жилых помещениях и собственниках (нанимателях) жилых помещений аварийного многоквартирного дома.docx");
        }

        public IActionResult MultiForma2(List<int> ids)
        {
            if (!ids.Any())
                return Error("Не выбрано ни одного помещения. Либо в здании таковые отсутствуют.");

            var fileNameReport = reportService.Forma2Ajax(ids);
            return Json(new { fileNameReport });
        }

        public IActionResult GetMultiForma2(string fileNameReport)
        {
            var file = reportService.DownloadFile(fileNameReport);
            return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                @"Форма 2. Сведения о жилых помещениях и собственниках (нанимателях) жилых помещений аварийного многоквартирного дома.docx");
        }

        public IActionResult GetForma3(int? idPremise)
        {
            if (idPremise == null)
                return Error("Помещение не выбрано.");

            var file = reportService.Forma3(new List<int>() { idPremise.Value });
            return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                @"Форма 3. Сведения, необходимые для целей формирования программы переселения граждан  из аварийных многоквартирных домов.docx");
        }

        public IActionResult MultiForma3(List<int> ids)
        {
            if (!ids.Any())
                return Error("Не выбрано ни одного помещения. Либо в здании таковые отсутствуют.");
            var fileNameReport = reportService.Forma3Ajax(ids);
            return Json(new { fileNameReport });
        }

        public IActionResult GetMultiForma3(string fileNameReport)
        {
            var file = reportService.DownloadFile(fileNameReport);
            return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                @"Форма 3. Сведения, необходимые для целей формирования программы переселения граждан  из аварийных многоквартирных домов.docx");
        }

        public IActionResult GetAzfAreaAnalize()
        {
            var file = reportService.AzfAreaAnalize();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                @"Анализ АЖФ на текущую дату (краткий) по площади.xlsx");
        }

        public IActionResult GetAzfRoomsAnalize()
        {
            var file = reportService.AzfRoomsAnalize();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                @"Анализ АЖФ на текущую дату (краткий) по количеству комнат.xlsx");
        }

        public IActionResult GetAzfRegionsAnalize()
        {
            var file = reportService.AzfRegionsAnalize();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                @"Анализ АЖФ на текущую дату (краткий) по жилым районам.xlsx");
        }

        public IActionResult GetAzfWithoutPrivAnalize()
        {
            var file = reportService.AzfWithoutPrivAnalize();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                @"Анализ АЖФ на текущую дату по МКД без частных ЖП.xlsx");
        }
    }
}