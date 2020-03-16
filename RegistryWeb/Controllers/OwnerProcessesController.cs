using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.Models.Entities;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerProcessesController : ListController<OwnerProcessesDataService>
    {
        ReportService reportService;

        public OwnerProcessesController(OwnerProcessesDataService dataService, SecurityService securityService, ReportService reportService)
            : base(dataService, securityService)
        {
            this.reportService = reportService;
        }

        public IActionResult Index(OwnerProcessesVM viewModel, bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<OwnerProcessesFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
            ViewBag.OwnerTypes = dataService.GetOwnerTypes();
            ViewBag.SecurityService = securityService;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        public IActionResult Create()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            return GetOwnerProcessView(dataService.CreateOwnerProcess());
        }

        [HttpPost]
        public IActionResult Create(OwnerProcess ownerProcess)
        {
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Create(ownerProcess);
                return RedirectToAction("Index");
            }
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpPost]
        public IActionResult AddressAdd(int id, string action)
        {
            return ViewComponent("AddressComponent", new { addressAssoc = new OwnerBuildingAssoc(), id, action });
        }

        [HttpPost]
        public IActionResult OwnerAdd(int idOwnerType, int id, string action)
        {
            var owner = new Owner() { IdOwnerType = idOwnerType };
            owner.IdOwnerTypeNavigation = dataService.GetOwnerType(idOwnerType);
            owner.OwnerReasons = new List<OwnerReason>() { new OwnerReason() };
            if (idOwnerType == 1)
                owner.OwnerPerson = new OwnerPerson();
            if (idOwnerType == 2 || idOwnerType == 3)
                owner.OwnerOrginfo = new OwnerOrginfo();
            return ViewComponent("OwnerComponent", new { owner, id, action });
        }

        [HttpPost]
        public IActionResult OwnerReasonAdd(int iOwner, int iReason, string action)
        {
            return ViewComponent("OwnerReasonComponent", new { ownerReason = new OwnerReason(), iOwner, iReason, action });
        }

        public IActionResult Details(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpGet]
        public IActionResult Delete(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpPost]
        public IActionResult Delete(OwnerProcess ownerProcess)
        {
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            dataService.Delete(ownerProcess.IdProcess);
            return RedirectToAction("Index");            
        }

        [HttpGet]
        public IActionResult Edit(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpPost]
        public IActionResult Edit(OwnerProcess ownerProcess)
        {
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Edit(ownerProcess);
                return RedirectToAction("Index");
            }
            return GetOwnerProcessView(ownerProcess);
        }
        
        public IActionResult Forma2(int? idProcess = null)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            if (!HttpContext.Session.Keys.Contains("idBuildings"))
                return Error("Не выбрано ни одного здания.");
            var ids = HttpContext.Session.Get<List<int>>("idBuildings");
            if (!ids.Any())
                return Error("Не выбрано ни одного здания.");
            try
            {
                var file = reportService.Forma2(ids);
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Форма 1. Общие сведения об аварийном многоквартирном доме г. Братск.docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        private IActionResult GetOwnerProcessView(OwnerProcess ownerProcess, [CallerMemberName]string action = "")
        {
            ViewBag.Action = action;
            var ownerProcessVM = new OwnerProcessVM();
            ownerProcessVM.OwnerProcess = ownerProcess;
            if (action == "Details" || action == "Edit")
                ownerProcessVM.Logs = dataService.GetProcessLog(ownerProcess.IdProcess);
            return View("OwnerProcessIndex", ownerProcessVM);
        }
    }
}