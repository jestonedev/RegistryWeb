using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using RegistryServices.ViewModel.Claims;
using RegistryDb.Models.Entities.Claims;
using RegistryServices.Enums;
using RegistryWeb.DataServices.Claims;
using RegistryServices.DataServices.Claims;
using RegistryWeb.Filters;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.ClaimsRead)]
    public class ClaimsController : ListController<ClaimsDataService, ClaimsFilter>
    {
        private readonly ClaimsAssignedAccountsDataService assignedAccountsService;

        public ClaimsController(ClaimsDataService dataService,
            ClaimsAssignedAccountsDataService assignedAccountsService, SecurityService securityService)
            : base(dataService, securityService)
        {
            nameFilteredIdsDict = "filteredClaimsIdsDict";
            nameIds = "idClaims";
            nameMultimaster = "ClaimsReports";
            this.assignedAccountsService = assignedAccountsService;
        }

        public IActionResult Index(ClaimsVM viewModel, bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<ClaimsFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
            ViewBag.SecurityService = securityService;
            ViewBag.SignersReports = dataService.Signers.Where(r => r.IdSignerGroup == 2).ToList();

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions, out List<int> filteredTenancyProcessesIds);

            AddSearchIdsToSession(vm.FilterOptions, filteredTenancyProcessesIds);

            return View(vm);
        }

        public IActionResult Details(int? idClaim, string returnUrl)
        {
            if (idClaim == null)
                return NotFound();
            var claim = dataService.GetClaim(idClaim.Value);
            if (claim == null)
                return NotFound();
            InitializeViewBag("Details", returnUrl, securityService.HasPrivilege(Privileges.ClaimsWrite));
            var claimVM = dataService.GetClaimViewModel(claim);
            return View("Claim", claimVM);
        }

        [HasPrivileges(Privileges.ClaimsWrite)]
        public ActionResult Create(int? idAccountBks, int? idAccountKumi)
        {
            InitializeViewBag("Create", null, true);
            return View("Claim", dataService.CreateClaimEmptyViewModel(idAccountBks, idAccountKumi));
        }

        [HttpPost]
        [HasPrivileges(Privileges.ClaimsWrite)]
        public ActionResult Create(ClaimVM claimVM)
        {
            if (claimVM == null || claimVM.Claim == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create( claimVM.Claim,
                    HttpContext.Request.Form.Files.Select(f => f).ToList(), claimVM.LoadPersonsSource);
                return RedirectToAction("Details", new { claimVM.Claim.IdClaim });
            }
            InitializeViewBag("Create", null, true);
            return View("Claim", dataService.GetClaimViewModel(claimVM.Claim));
        }

        public ActionResult GetClaimPersonsBySource(LoadPersonsSourceEnum loadPersonsSource, int? idAccountBks, int? idAccountKumi)
        {
            var persons = new List<ClaimPerson>();
            switch (loadPersonsSource)
            {
                case LoadPersonsSourceEnum.Tenancy:
                    persons = dataService.GetClaimPersonsFromTenancy(idAccountBks, idAccountKumi);
                    break;
                case LoadPersonsSourceEnum.PrevClaim:
                    persons = dataService.GetClaimPersonsFromPrevClaim(idAccountBks, idAccountKumi);
                    break;
            }
            return PartialView("ClaimPersonsPreview", persons);
        }

        [HasPrivileges(Privileges.ClaimsWrite)]
        public ActionResult Edit(int? idClaim, string returnUrl)
        {
            if (idClaim == null)
                return NotFound();

            var claim = dataService.GetClaim(idClaim.Value);
            if (claim == null)
                return NotFound();

            InitializeViewBag("Edit", returnUrl, true);
            return View("Claim", dataService.GetClaimViewModel(claim));
        }

        [HttpPost]
        [HasPrivileges(Privileges.ClaimsWrite)]
        public ActionResult Edit(ClaimVM claimVM, string returnUrl)
        {
            if (claimVM == null || claimVM.Claim == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                dataService.Edit(claimVM.Claim);
                return RedirectToAction("Details", new { claimVM.Claim.IdClaim });
            }
            InitializeViewBag("Edit", returnUrl, true);
            return View("Claim", dataService.GetClaimViewModel(claimVM.Claim));
        }

        [HasPrivileges(Privileges.ClaimsWrite)]
        public ActionResult Delete(int? idClaim, string returnUrl)
        {
            if (idClaim == null)
                return NotFound();

            var claim = dataService.GetClaim(idClaim.Value);
            if (claim == null)
                return NotFound();

            InitializeViewBag("Delete", returnUrl, false);
            return View("Claim", dataService.GetClaimViewModel(claim));
        }

        [HttpPost]
        [HasPrivileges(Privileges.ClaimsWrite)]
        public ActionResult Delete(ClaimVM claimVM)
        {
            if (claimVM == null || claimVM.Claim == null)
                return NotFound();

            dataService.Delete(claimVM.Claim.IdClaim);
            return RedirectToAction("Index");
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
            ViewBag.SignersReports = dataService.Signers.Where(r => r.IdSignerGroup == 2).ToList();
        }

        [HttpPost]
        public JsonResult GetAccounts(string text, string type, bool excludeAnnual)
        {
            var accounts = assignedAccountsService.GetAccounts(text, type, excludeAnnual);
            return Json(accounts.Select(pa => new {
                pa.IdAccount,
                pa.Account
            }));
        }

        [HttpPost]
        public JsonResult GetAccountInfo(int idAccount, string type)
        {
            if (type == "BKS")
            {
                var account = assignedAccountsService.GetAccountBks(idAccount);
                var rentObjects = assignedAccountsService.GetRentObjectsBks(new List<int> { idAccount });
                var lastPaymentInfo = assignedAccountsService.GetLastPaymentsInfo(new List<int> { idAccount }).Select(v => v.Value).FirstOrDefault();
                return Json(new
                {
                    PaymentAccount = account?.Account,
                    BksAddress = account?.RawAddress,
                    RegistryAddress = rentObjects.ContainsKey(idAccount) ? rentObjects[idAccount].Select(r => r.Text).Aggregate((acc, v) => acc + ", " + v) : "",
                    AmountTenancy = lastPaymentInfo?.BalanceOutputTenancy,
                    AmountPenalties = lastPaymentInfo?.BalanceOutputPenalties,
                    AmountDgi = lastPaymentInfo?.BalanceOutputDgi,
                    AmountPadun = lastPaymentInfo?.BalanceOutputPadun,
                    AmountPkk = lastPaymentInfo?.BalanceOutputPkk
                });
            } else
            if (type == "KUMI")
            {
                var account = assignedAccountsService.GetAccountKumi(idAccount);
                var address = assignedAccountsService.GetAccountAddress(idAccount);
                
                return Json(new
                {
                    RegistryAddress = address,
                    AmountTenancy = account.CurrentBalanceTenancy,
                    AmountPenalties = account.CurrentBalancePenalty,
                    AmountDgi = account.CurrentBalanceDgi,
                    AmountPkk = account.CurrentBalancePkk,
                    AmountPadun = account.CurrentBalancePadun
                });
            }
            return Json(null);
        }

        [HttpPost]
        [HasPrivileges(Privileges.ClaimsWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddClaimState(string action, int idClaim)
        {
            var claim= dataService.GetClaim(idClaim) ?? new Claim { ClaimStates = new List<ClaimState>(), ClaimCourtOrders = new List<ClaimCourtOrder>() };
            var claimStates = claim.ClaimStates;
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.StateTypes = dataService.StateTypes;
            ViewBag.StateTypeRelations = dataService.StateTypeRelations;
            claimStates.Add(new ClaimState {
                Executor = dataService.CurrentExecutor?.ExecutorName,
                DateStartState = DateTime.Now.Date,
                BksRequester = dataService.CurrentExecutor?.ExecutorName,
                TransferToLegalDepartmentWho = dataService.CurrentExecutor?.ExecutorName,
                AcceptedByLegalDepartmentWho = dataService.CurrentExecutor?.ExecutorName
            });
            ViewBag.ClaimStateIndex = claimStates.Count - 1;

            ViewBag.ClaimCourtOrders = claim.ClaimCourtOrders;
            ViewBag.Executors = dataService.Executors.Where(r => action == "Create" || !r.IsInactive).ToList();
            ViewBag.Judges = dataService.Judges.Where(r => action == "Create" || !r.IsInactive).ToList();
            ViewBag.Signers = dataService.Signers.Where(r => r.IdSignerGroup == 3).ToList();
            return PartialView("ClaimState", claimStates);
        }

        [HttpPost]
        [HasPrivileges(Privileges.ClaimsWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddCourtOrder(string action, int idAccount)
        {
            var courtOrder = new ClaimCourtOrder {
                IdExecutor = dataService.CurrentExecutor?.IdExecutor ?? 0,
                CreateDate = DateTime.Now.Date,
                IdJudge = dataService.GetIdJudge(idAccount)
            };

            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.Executors = dataService.Executors.Where(r => action == "Create" || !r.IsInactive).ToList();
            ViewBag.Judges = dataService.Judges.Where(r => action == "Create" || !r.IsInactive).ToList();
            ViewBag.Signers = dataService.Signers.Where(r => r.IdSignerGroup == 3).ToList();
            return PartialView("ClaimCourtOrder", courtOrder);
        }

        [HttpPost]
        [HasPrivileges(Privileges.ClaimsWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddClaimFile(string action)
        {
            var file = new ClaimFile { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;

            return PartialView("AttachmentFile", file);
        }

        [HttpPost]
        [HasPrivileges(Privileges.ClaimsWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult AddClaimPerson(string action)
        {
            var file = new ClaimPerson { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;

            return PartialView("ClaimPerson", file);
        }

        public IActionResult ClaimsReports(PageOptions pageOptions)
        {
            var errorIds = new List<int>();
            if (TempData.ContainsKey("ErrorClaimsIds"))
            {
                try
                {
                    errorIds = JsonConvert.DeserializeObject<List<int>>(TempData["ErrorClaimsIds"].ToString());
                }
                catch
                {
                }
                TempData.Remove("ErrorClaimsIds");
            }
            if (TempData.ContainsKey("ErrorReason"))
            {
                ViewBag.ErrorReason = TempData["ErrorReason"];
                TempData.Remove("ErrorReason");
            } else
            {
                ViewBag.ErrorReason = "неизвестно";
            }

            ViewBag.ErrorClaims = dataService.GetClaimsForMassReports(errorIds).ToList();

            var ids = GetSessionIds();
            var viewModel = dataService.GetClaimsViewModelForMassReports(ids, pageOptions);
            ViewBag.Count = viewModel.Claims.Count();
            ViewBag.SignersReports = dataService.Signers.Where(r => r.IdSignerGroup == 2).ToList();
            ViewBag.CurrentExecutor = dataService.CurrentExecutor?.ExecutorName;
            ViewBag.StateTypes = dataService.StateTypes;
            ViewBag.CanEdit = securityService.HasPrivilege(Privileges.ClaimsWrite);
            return View("ClaimsReports", viewModel);
        }

        [HasPrivileges(Privileges.ClaimsWrite, Model = "У вас нет прав на выполнение данной операции", ViewResultType = typeof(ViewResult), ViewName = "Error")]
        public IActionResult UpdateDeptPeriod(DateTime? startDeptPeriod, DateTime? endDeptPeriod)
        {
            var ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            dataService.UpdateDeptPeriodInClaims(ids, startDeptPeriod, endDeptPeriod);

            return RedirectToAction("ClaimsReports");
        }

        [HasPrivileges(Privileges.ClaimsWrite, Model = "У вас нет прав на выполнение данной операции", ViewResultType = typeof(ViewResult), ViewName = "Error")]
        public IActionResult AddClaimStateMass(ClaimState claimState)
        {
            var ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();


            var processingIds = new List<int>();
            var errorIds = new List<int>();

            foreach (var idClaim in ids)
            {
                var claimStates = dataService.GetClaimStates(idClaim);
                var claimStateTypeRelations = dataService.StateTypeRelations;
                var claimStateTypes = dataService.StateTypes;
                ClaimState prevClaimState = null;
                if (claimStates.Count > 0)
                {
                    prevClaimState = claimStates[claimStates.Count - 1];
                }
                if ((prevClaimState == null &&
                    !claimStateTypes.Any(r => r.IsStartStateType && r.IdStateType == claimState.IdStateType)) ||
                    (prevClaimState != null &&
                    !claimStateTypeRelations.Any(r => r.IdStateFrom == prevClaimState.IdStateType && r.IdStateTo == claimState.IdStateType)))
                {
                    errorIds.Add(idClaim);
                } else
                {
                    processingIds.Add(idClaim);
                }
            }

            dataService.AddClaimStateMass(processingIds, claimState);

            TempData["ErrorClaimsIds"] = JsonConvert.SerializeObject(errorIds);
            TempData["ErrorReason"] = "нарушение порядка следования этапов исковой работы";
            return RedirectToAction("ClaimsReports");
        }
    }
}