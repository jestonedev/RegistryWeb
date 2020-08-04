using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.Extensions;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Models.Entities;
using RegistryWeb.Models;

namespace RegistryWeb.Controllers
{
    public class TenancyProcessesController : ListController<TenancyProcessesDataService>
    {
        public TenancyProcessesController(TenancyProcessesDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
        }

        // GET: TenancyProcesses
        public ActionResult Index(TenancyProcessesVM viewModel, bool isBack = false)
        {
            if(viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<TenancyProcessesFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
            ViewBag.SecurityService = securityService;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        // GET: TenancyProcesses/Details/5
        public ActionResult Details(int? idProcess, string returnUrl)
        {
            ViewBag.Action = "Details";
            ViewBag.ReturnUrl = returnUrl;
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            var process = dataService.GetTenancyProcess(idProcess.Value);
            if (process == null)
                return NotFound();
            ViewBag.CanEditBaseInfo = securityService.HasPrivilege(Privileges.TenancyWrite);
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(process));
        }
        
        public ActionResult Create()
        {
            ViewBag.Action = "Create";
            ViewBag.SecurityService = securityService;
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return View("NotAccess");
            ViewBag.CanEditBaseInfo = true;
            return View("TenancyProcess", dataService.CreateTenancyProcessEmptyViewModel());
        }
        
        [HttpPost]
        public ActionResult Create(TenancyProcessVM tenancyProcessVM)
        {
            if (tenancyProcessVM == null || tenancyProcessVM.TenancyProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Create(tenancyProcessVM.TenancyProcess, tenancyProcessVM.RentObjects,
                    HttpContext.Request.Form.Files.Select(f => f).ToList());
                return RedirectToAction("Details", new { tenancyProcessVM.TenancyProcess.IdProcess });
            }
            ViewBag.Action = "Create";
            ViewBag.SecurityService = securityService;
            ViewBag.CanEditBaseInfo = true;
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcessVM.TenancyProcess));
        }
        
        public ActionResult Edit(int? idProcess, string returnUrl)
        {
            ViewBag.Action = "Edit";
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            if (idProcess == null)
                return NotFound();

            var tenancyProcess = dataService.GetTenancyProcess(idProcess.Value);
            if (tenancyProcess == null)
                return NotFound();

            ViewBag.CanEditBaseInfo = securityService.HasPrivilege(Privileges.TenancyWrite);

            if (!(bool)ViewBag.CanEditBaseInfo)
                return View("NotAccess");
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcess));
        }
        
        [HttpPost]
        public ActionResult Edit(TenancyProcessVM tenancyProcessVM, string returnUrl)
        {
            if (tenancyProcessVM == null || tenancyProcessVM.TenancyProcess == null)
                return NotFound();

            ViewBag.CanEditBaseInfo = securityService.HasPrivilege(Privileges.TenancyWrite);

            if (!(bool)ViewBag.CanEditBaseInfo)
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                tenancyProcessVM.TenancyProcess.TenancyRentPeriods = null;
                dataService.Edit(tenancyProcessVM.TenancyProcess);
                return RedirectToAction("Details", new { tenancyProcessVM.TenancyProcess.IdProcess });
            }
            ViewBag.Action = "Edit";
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcessVM.TenancyProcess));
        }
        
        public ActionResult Delete(int? idProcess, string returnUrl)
        {
            ViewBag.Action = "Delete";
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            if (idProcess == null)
                return NotFound();

            var tenancyProcess = dataService.GetTenancyProcess(idProcess.Value);
            if (tenancyProcess == null)
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return View("NotAccess");

            ViewBag.CanEditBaseInfo = false;

            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcess));
        }
        
        [HttpPost]
        public ActionResult Delete(TenancyProcessVM tenancyProcessVM)
        {
            if (tenancyProcessVM == null || tenancyProcessVM.TenancyProcess == null)
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return View("NotAccess");

            dataService.Delete(tenancyProcessVM.TenancyProcess.IdProcess);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddRentPeriod(string action)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(-2);

            var rentPeriod = new TenancyRentPeriod { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;

            return PartialView("RentPeriod", rentPeriod);
        }

        [HttpPost]
        public IActionResult AddTenancyReason(string action)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(-2);

            var reason = new TenancyReason { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;
            ViewBag.TenancyReasonTypes = dataService.TenancyReasonTypes;

            return PartialView("TenancyReason", reason);
        }

        [HttpPost]
        public IActionResult AddTenancyPerson(string action)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(-2);

            var person = new TenancyPerson { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;
            ViewBag.Kinships = dataService.Kinships;

            return PartialView("TenancyPerson", person);
        }

        [HttpPost]
        public IActionResult AddTenancyAgreement(string action)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(-2);

            var agreement = new TenancyAgreement { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;
            ViewBag.Executors = dataService.ActiveExecutors;

            return PartialView("TenancyAgreement", agreement);
        }

        [HttpPost]
        public IActionResult AddRentObject(string action)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(-2);

            var rentObject = new TenancyRentObject { Address = new Address {
                AddressType = AddressTypes.None
            } };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;
            ViewBag.Streets = dataService.Streets;

            return PartialView("RentObject", rentObject);
        }

        [HttpPost]
        public IActionResult AddTenancyFile(string action)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return Json(-2);

            var file = new TenancyFile { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;

            return PartialView("AttachmentFile", file);
        }
    }
}