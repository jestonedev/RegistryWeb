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
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class TenancyProcessesController : ListController<TenancyProcessesDataService, TenancyProcessesFilter>
    {
        public TenancyProcessesController(TenancyProcessesDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
            nameFilteredIdsDict = "filteredTenancyProcessesIdsDict";
            nameIds = "idTenancyProcesses";
            nameMultimaster = "TenancyProcessesReports";
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
            ViewBag.DistrictCommittees = new SelectList(dataService.DistrictCommittees, "IdCommittee", "NameNominative");
            ViewBag.DistrictCommitteesPreambles = new SelectList(dataService.DistrictCommitteesPreContractPreambles, "IdPreamble", "Name");

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions, out List<int> filteredTenancyProcessesIds);

            AddSearchIdsToSession(vm.FilterOptions, filteredTenancyProcessesIds);

            return View(vm);
        }

        private void InitializeViewBag(string action, string returnUrl, bool canEditBaseInfo)
        {
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = canEditBaseInfo;
            if (returnUrl != null)
            {
                ViewBag.ReturnUrl = returnUrl;
            }
            ViewBag.SecurityService = securityService;
            ViewBag.DistrictCommittees = new SelectList(dataService.DistrictCommittees, "IdCommittee", "NameNominative");
            ViewBag.DistrictCommitteesPreambles = new SelectList(dataService.DistrictCommitteesPreContractPreambles, "IdPreamble", "Name");
        }

        // GET: TenancyProcesses/Details/5
        public ActionResult Details(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            var process = dataService.GetTenancyProcess(idProcess.Value);
            if (process == null)
                return NotFound();
            InitializeViewBag("Details", returnUrl, securityService.HasPrivilege(Privileges.TenancyWrite));
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(process));
        }
        
        public ActionResult Create()
        {
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return View("NotAccess");
            InitializeViewBag("Create", null, true);
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
            InitializeViewBag("Create", null, true);
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcessVM.TenancyProcess));
        }
        
        public ActionResult Edit(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();

            var tenancyProcess = dataService.GetTenancyProcess(idProcess.Value);
            if (tenancyProcess == null)
                return NotFound();

            var canEditBaseInfo = securityService.HasPrivilege(Privileges.TenancyWrite);

            if (!canEditBaseInfo)
                return View("NotAccess");

            InitializeViewBag("Edit", returnUrl, canEditBaseInfo);
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcess));
        }
        
        [HttpPost]
        public ActionResult Edit(TenancyProcessVM tenancyProcessVM, string returnUrl)
        {
            if (tenancyProcessVM == null || tenancyProcessVM.TenancyProcess == null)
                return NotFound();

            var canEditBaseInfo = securityService.HasPrivilege(Privileges.TenancyWrite);

            if (!canEditBaseInfo)
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                tenancyProcessVM.TenancyProcess.TenancyRentPeriods = null;
                dataService.Edit(tenancyProcessVM.TenancyProcess);
                return RedirectToAction("Details", new { tenancyProcessVM.TenancyProcess.IdProcess });
            }
            InitializeViewBag("Edit", returnUrl, canEditBaseInfo);
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcessVM.TenancyProcess));
        }
        
        public ActionResult Delete(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();

            var tenancyProcess = dataService.GetTenancyProcess(idProcess.Value);
            if (tenancyProcess == null)
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
                return View("NotAccess");
            
            InitializeViewBag("Delete", returnUrl, false);

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

            var agreement = new TenancyAgreement {
                IdExecutor = dataService.ActiveExecutors.FirstOrDefault(e => e.ExecutorLogin != null &&
                        e.ExecutorLogin.ToLowerInvariant() == securityService.User.UserName.ToLowerInvariant())?.IdExecutor,
                AgreementDate = DateTime.Now.Date,
                AgreementContent = @"1.1. По настоящему Соглашению Стороны по договору № {0} от {1} {2} найма жилого помещения, расположенного по адресу: {3}, договорились:"
            };
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

        [HttpPost]
        public IActionResult UpdateRentPeriod(int? idProcess, DateTime? beginDate, DateTime? endDate, bool untilDismissal)
        {
            if (idProcess == null || idProcess == 0)
            {
                return Json(new
                {
                    Code = -1,
                    Text = "Не удалось найти процесс найма"
                });
            }
            if (!securityService.HasPrivilege(Privileges.TenancyWrite))
            {
                return Json(new
                {
                    Code = -1,
                    Text = "У вас нет прав на редактирование найма жилья"
                });
            }
            dataService.UpdateExcludeDate(idProcess, beginDate, endDate, untilDismissal);
            return Json(new
            {
                Code = 0
            });
        }      

        public IActionResult TenancyProcessesReports(PageOptions pageOptions)
        {
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            var ids = GetSessionIds();
            var viewModel = dataService.GetTenancyProcessesViewModelForMassReports(ids, pageOptions);
            ViewBag.Count = viewModel.TenancyProcesses.Count();
            ViewBag.PreparersSelectList = new SelectList(dataService.Preparers, "IdPreparer", "PreparerName");
            ViewBag.TenancyReasonTypesSelectList = new SelectList(dataService.TenancyReasonTypes, "IdReasonType", "ReasonName");
            ViewBag.CanEdit = securityService.HasPrivilege(Privileges.TenancyWrite);
            return View("TenancyProcessesReports", viewModel);
        }
    }
}