using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryDb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.Owners;
using RegistryDb.Models.Entities.Owners;

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
            return GetOwnerProcessView(dataService.CreateOwnerProcess(filterOptions), returnUrl);
        }

        [HttpPost]
        public IActionResult Create(OwnerProcess ownerProcess, string returnUrl, IFormFileCollection attachmentFiles)
        {
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Create(ownerProcess, attachmentFiles);
                return RedirectToAction("Details", new { ownerProcess.IdProcess, returnUrl });
            }
            return Redirect(returnUrl);
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
            ownerVM.I = i;
            ownerVM.Action = action;
            return PartialView("Owner", ownerVM);
        }

        [HttpPost]
        public IActionResult OwnerReasonAdd(int i, int j, ActionTypeEnum action)
        {
            var ownerReasonVM = new OwnerReasonVM();
            ownerReasonVM.OwnerReason = new OwnerReason();
            ownerReasonVM.OwnerReason.NumeratorShare = 1;
            ownerReasonVM.OwnerReason.DenominatorShare = 1;
            ownerReasonVM.OwnerReason.IdReasonType = 9; //Государственная регистрация права (ЕГРП)
            ownerReasonVM.OwnerReason.ReasonDate = DateTime.Now;
            ownerReasonVM.I = i;
            ownerReasonVM.J = j;
            ownerReasonVM.Action = action;
            ViewBag.OwnerReasonTypes = dataService.OwnerReasonTypes();
            return PartialView("OwnerReason", ownerReasonVM);
        }

        public IActionResult OwnerFileAdd(int i, ActionTypeEnum action)
        {
            var ownerFileVM = new OwnerFileVM()
            {
                OwnerFile = new OwnerFile() { DateDownload = DateTime.Now },
                Action = action,
                I = i
            };
            return PartialView("OwnerFile", ownerFileVM);
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
        public IActionResult Delete(OwnerProcess ownerProcess, string returnUrl)
        {
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            dataService.Delete(ownerProcess.IdProcess);
            return Redirect(returnUrl);            
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
        public IActionResult Edit(OwnerProcess ownerProcess, string returnUrl, IFormFileCollection attachmentFiles, bool[] removeFiles)
        {
            var r = Request;
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {                
                dataService.Edit(ownerProcess, attachmentFiles, removeFiles);
                return RedirectToAction("Edit", new { ownerProcess.IdProcess, returnUrl });
            }
            return Redirect(returnUrl);
        }

        private IActionResult GetOwnerProcessView(OwnerProcess ownerProcess, string returnUrl, [CallerMemberName]string action = "")
        {
            var actionType = (ActionTypeEnum)Enum.Parse(typeof(ActionTypeEnum), action);
            ViewBag.Action = actionType;
            ViewBag.OwnerReasonTypes = dataService.OwnerReasonTypes();
            ViewBag.ReturnUrl = returnUrl;        
            return View("OwnerProcess", ownerProcess);
        }

        public IActionResult DownloadFile(int id)
        {
            var ownerFile = dataService.GetOwnerFile(id);           
            if (ownerFile == null) return Json(new { Error = -1 });
            var filePath = Path.Combine(dataService.AttachmentsPath, ownerFile.FileOriginName);
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { Error = -2 });
            }
            return File(System.IO.File.ReadAllBytes(filePath), ownerFile.FileMimeType, ownerFile.FileDisplayName);
        }
    }
}