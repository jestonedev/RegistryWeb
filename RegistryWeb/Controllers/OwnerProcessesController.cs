using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerProcessesController : ListController<OwnerProcessesDataService, OwnerProcessesFilter>
    {
        public OwnerProcessesController(OwnerProcessesDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
        }

        public IActionResult Index(OwnerProcessesVM viewModel)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            ViewBag.KladrStreets = dataService.KladrStreets();
            ViewBag.OwnerTypes = dataService.OwnerTypes();
            ViewBag.SecurityService = securityService;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        [HttpGet]
        public IActionResult Create(OwnerProcessesFilter filterOptions, string returnUrl)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            ViewBag.ReturnUrl = returnUrl;
            return GetOwnerProcessView(dataService.CreateOwnerProcess(filterOptions), returnUrl);
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
                return RedirectToAction("Details", new { ownerProcess.IdProcess });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddressAdd(int idProcess, int id, int i, string text, AddressTypes addressType, ActionTypeEnum action)
        {
            var addressVM = new OwnerProcessAddressVM();
            addressVM.AddressType = addressType;
            addressVM.Action = action;
            addressVM.IdProcess = idProcess;
            addressVM.IdAssoc = 0;
            addressVM.Id = id;
            addressVM.I = i;
            addressVM.Text = text;
            addressVM.Href = dataService.GetHrefToReestr(id, addressType);
            return PartialView("OwnerProcessAddress", addressVM);
        }

        [HttpPost]
        public IActionResult OwnerAdd(int idOwnerType, int i, ActionTypeEnum action)
        {
            var ownerVM = new OwnerVM();
            ownerVM.Owner = new Owner() { IdOwnerType = idOwnerType };
            ownerVM.Owner.IdOwnerTypeNavigation = dataService.GetOwnerType(idOwnerType);
            if (idOwnerType == 1)
                ownerVM.Owner.OwnerPerson = new OwnerPerson();
            if (idOwnerType == 2 || idOwnerType == 3)
                ownerVM.Owner.OwnerOrginfo = new OwnerOrginfo();
            ownerVM.Owner.OwnerReasons = new List<OwnerReason>() { new OwnerReason() };
            ownerVM.I = i;
            ownerVM.Action = action;
            ViewBag.OwnerReasonTypes = dataService.OwnerReasonTypes();
            return PartialView("Owner", ownerVM);
        }

        [HttpPost]
        public IActionResult OwnerReasonAdd(int iOwner, int iReason, ActionTypeEnum action)
        {
            var ownerReasonVM = new OwnerReasonVM();
            ownerReasonVM.OwnerReason = new OwnerReason();
            ownerReasonVM.IOwner = iOwner;
            ownerReasonVM.IReason = iReason;
            ownerReasonVM.Action = action;
            ViewBag.OwnerReasonTypes = dataService.OwnerReasonTypes();
            return PartialView("OwnerReason", ownerReasonVM);
        }

        [HttpPost]
        public JsonResult AddressInfoGet(int id, AddressTypes addressType)
        {
            var addressInfo = dataService.GetAddressInfo(id, addressType);
            return Json(addressInfo);
        }

        [HttpPost]
        public IActionResult ProcessLogGet(int idProcess)
        {
            var processLog = dataService.GetProcessLog(idProcess);
            return PartialView("OwnerProcessLog", processLog);
        }

        [HttpGet]
        public IActionResult Details(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            ViewBag.ReturnUrl = returnUrl;
            return GetOwnerProcessView(ownerProcess, returnUrl);
        }

        [HttpGet]
        public IActionResult Delete(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess, returnUrl);
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
        public IActionResult Edit(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess, returnUrl);
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
                return RedirectToAction("Edit", new { ownerProcess.IdProcess });
            }
            return RedirectToAction("Index");
        }

        private IActionResult GetOwnerProcessView(OwnerProcess ownerProcess, string returnUrl, [CallerMemberName]string action = "")
        {
            var actionType = (ActionTypeEnum)Enum.Parse(typeof(ActionTypeEnum), action);
            ViewBag.Action = actionType;
            ViewBag.OwnerReasonTypes = dataService.OwnerReasonTypes();
            ViewBag.ReturnUrl = returnUrl;        
            return View("OwnerProcess", ownerProcess);
        }
    }
}