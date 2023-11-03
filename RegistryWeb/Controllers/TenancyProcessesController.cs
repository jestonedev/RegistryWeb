using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.Extensions;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.Tenancies;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.Filters;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.TenancyRead)]
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
            InitializeViewBagCommonDictionaries();

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions, out List<int> filteredTenancyProcessesIds);

            AddSearchIdsToSession(vm.FilterOptions, filteredTenancyProcessesIds);

            return View(vm);
        }

        private void InitializeViewBagCommonDictionaries()
        {
            ViewBag.SecurityService = securityService;
            ViewBag.DistrictCommittees = new SelectList(dataService.DistrictCommittees, "IdCommittee", "NameNominative");
            ViewBag.DistrictCommitteesPreambles = new SelectList(dataService.DistrictCommitteesPreContractPreambles, "IdPreamble", "Name");
            ViewBag.PreparersSelectList = new SelectList(dataService.Preparers, "IdPreparer", "PreparerName");
            ViewBag.BksSignersSelectList = new SelectList(dataService.BksSigners.Where(s => s.IdSignerGroup == 1).ToList().Select(s => new {
                s.IdRecord,
                Snp = s.Surname + " " + s.Name + (s.Patronymic == null ? "" : " " + s.Patronymic)
            }), "IdRecord", "Snp");
        }

        private void InitializeViewBag(string action, string returnUrl, bool canEditBaseInfo, bool canEditEmailsOnly)
        {
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = canEditBaseInfo;
            ViewBag.CanEditEmailsOnly = canEditEmailsOnly;
            if (returnUrl != null)
            {
                ViewBag.ReturnUrl = returnUrl;
            }
            InitializeViewBagCommonDictionaries();
            ViewBag.AreaAvgCostActualDate = dataService.AreaAvgCostActualDate;
        }

        // GET: TenancyProcesses/Details/5
        public ActionResult Details(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();
            var process = dataService.GetTenancyProcess(idProcess.Value);
            if (process == null)
                return NotFound();
            InitializeViewBag("Details", returnUrl, securityService.HasPrivilege(Privileges.TenancyWrite), securityService.HasPrivilege(Privileges.TenancyWriteEmailsOnly));
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(process));
        }

        [HasPrivileges(Privileges.TenancyWrite)]
        public ActionResult Create(int? idProcess, int? idObject, AddressTypes addressType = AddressTypes.None)
        {
            InitializeViewBag("Create", null, true, true);
            var vm = dataService.CreateTenancyProcessEmptyViewModel(idObject, addressType);
            if (idProcess != null)
            {
                vm = dataService.ClearifyTenancyProcessVmForCopy(dataService.GetTenancyProcessViewModel(dataService.GetTenancyProcess(idProcess.Value)));
            }
            return View("TenancyProcess", vm);
        }
        
        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite)]
        public ActionResult Create(TenancyProcessVM tenancyProcessVM)
        {
            if (tenancyProcessVM == null || tenancyProcessVM.TenancyProcess == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create(tenancyProcessVM.TenancyProcess, tenancyProcessVM.RentObjects,
                    HttpContext.Request.Form.Files.Select(f => f).ToList());
                return RedirectToAction("Details", new { tenancyProcessVM.TenancyProcess.IdProcess });
            }
            InitializeViewBag("Create", null, true, true);
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcessVM.TenancyProcess));
        }

        [HasPrivileges(Privileges.TenancyWrite, Privileges.TenancyWriteEmailsOnly, PrivilegesComparator = Filters.Common.PrivilegesComparator.Or)]
        public ActionResult Edit(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();

            var tenancyProcess = dataService.GetTenancyProcess(idProcess.Value);
            if (tenancyProcess == null)
                return NotFound();

            var canEditBaseInfo = securityService.HasPrivilege(Privileges.TenancyWrite);
            var canEditEmailsOnly = securityService.HasPrivilege(Privileges.TenancyWriteEmailsOnly);

            InitializeViewBag("Edit", returnUrl, canEditBaseInfo, canEditEmailsOnly);
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcess));
        }
        
        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite)]
        public ActionResult Edit(TenancyProcessVM tenancyProcessVM, string returnUrl)
        {
            if (tenancyProcessVM == null || tenancyProcessVM.TenancyProcess == null)
                return NotFound();

            var canEditBaseInfo = securityService.HasPrivilege(Privileges.TenancyWrite);
            var canEditEmailsOnly = securityService.HasPrivilege(Privileges.TenancyWriteEmailsOnly);

            if (ModelState.IsValid)
            {
                tenancyProcessVM.TenancyProcess.TenancyRentPeriods = null;
                dataService.Edit(tenancyProcessVM.TenancyProcess);
                return RedirectToAction("Details", new { tenancyProcessVM.TenancyProcess.IdProcess });
            }
            InitializeViewBag("Edit", returnUrl, canEditBaseInfo, canEditEmailsOnly);
            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcessVM.TenancyProcess));
        }

        [HasPrivileges(Privileges.TenancyWrite)]
        public ActionResult Delete(int? idProcess, string returnUrl)
        {
            if (idProcess == null)
                return NotFound();

            var tenancyProcess = dataService.GetTenancyProcess(idProcess.Value);
            if (tenancyProcess == null)
                return NotFound();
            
            InitializeViewBag("Delete", returnUrl, false, false);

            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(tenancyProcess));
        }
        
        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite)]
        public ActionResult Delete(TenancyProcessVM tenancyProcessVM)
        {
            if (tenancyProcessVM == null || tenancyProcessVM.TenancyProcess == null)
                return NotFound();

            dataService.Delete(tenancyProcessVM.TenancyProcess.IdProcess);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite, Model = -2, ViewResultType = typeof(JsonResult))]
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
        [HasPrivileges(Privileges.TenancyWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddTenancyReason(string action)
        {
            var reason = new TenancyReason { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;
            ViewBag.TenancyReasonTypes = dataService.TenancyReasonTypes;

            return PartialView("TenancyReason", reason);
        }

        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddTenancyPerson(string action)
        {
            var person = new TenancyPerson { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;
            ViewBag.Kinships = dataService.Kinships;

            return PartialView("TenancyPerson", person);
        }

        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddTenancyAgreement(string action)
        {
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
        [HasPrivileges(Privileges.TenancyWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddRentObject(string action)
        {
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
        [HasPrivileges(Privileges.TenancyWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddTenancyFile(string action)
        {
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

        [HttpPost]
        public IActionResult RegNumExist(string regNum, int idProcess)
        {
            if (regNum == null)
                return Json(false);
            var isExist = dataService.RegNumExist(regNum, idProcess);
            return Json(isExist);
        }

        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddEmployer(string employerName)
        {
            if (string.IsNullOrEmpty(employerName))
                return Json(-1);
            var duplicate = dataService.GetEmploeyerByName(employerName);
                          
            if (duplicate != null)
            {
                return Json(-3);
            }
            var employer = new Employer { EmployerName = employerName };
            dataService.AddEmployer(employer);
            return Json(employer.IdEmployer);
        }

        public IActionResult TenancyProcessesReports(PageOptions pageOptions)
        {
            var ids = GetSessionIds();
            var viewModel = dataService.GetTenancyProcessesViewModelForMassReports(ids, pageOptions);
            ViewBag.Count = viewModel.TenancyProcesses.Count();
            ViewBag.PreparersSelectList = new SelectList(dataService.Preparers, "IdPreparer", "PreparerName");
            ViewBag.TenancyReasonTypesSelectList = new SelectList(dataService.TenancyReasonTypes, "IdReasonType", "ReasonName");
            ViewBag.CanEdit = securityService.HasPrivilege(Privileges.TenancyWrite);
            return View("TenancyProcessesReports", viewModel);
        }

        public IActionResult PaymentHistory(int id, PaymentHistoryTarget target)
        {
            ViewBag.Title = dataService.GetPaymentHistoryTitle(id, target);
            ViewBag.Target = target;
            var viewModel = dataService.GetPaymentHistory(id, target);
            return View(viewModel);
        }
    }
}