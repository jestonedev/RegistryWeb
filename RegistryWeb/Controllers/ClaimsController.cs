using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class ClaimsController : ListController<ClaimsDataService, ClaimsFilter>
    {
        public ClaimsController(ClaimsDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
            nameFilteredIdsDict = "filteredClaimsIdsDict";
            nameIds = "idClaims";
            nameMultimaster = "ClaimsReports";
        }

        public IActionResult Index(ClaimsVM viewModel, bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
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
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");
            var claim = dataService.GetClaim(idClaim.Value);
            if (claim == null)
                return NotFound();
            InitializeViewBag("Details", returnUrl, securityService.HasPrivilege(Privileges.ClaimsWrite));
            var claimVM = dataService.GetClaimViewModel(claim);
            return View("Claim", claimVM);
        }

        public ActionResult Create(int? idAccountBks, int? idAccountKumi)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return View("NotAccess");
            InitializeViewBag("Create", null, true);
            return View("Claim", dataService.CreateClaimEmptyViewModel(idAccountBks, idAccountKumi));
        }

        [HttpPost]
        public ActionResult Create(ClaimVM claimVM)
        {
            if (claimVM == null || claimVM.Claim == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Create(claimVM.Claim,
                    HttpContext.Request.Form.Files.Select(f => f).ToList());
                return RedirectToAction("Details", new { claimVM.Claim.IdClaim });
            }
            InitializeViewBag("Create", null, true);
            return View("Claim", dataService.GetClaimViewModel(claimVM.Claim));
        }

        public ActionResult Edit(int? idClaim, string returnUrl)
        {
            if (idClaim == null)
                return NotFound();

            var claim = dataService.GetClaim(idClaim.Value);
            if (claim == null)
                return NotFound();

            var canEditBaseInfo = securityService.HasPrivilege(Privileges.ClaimsWrite);

            if (!canEditBaseInfo)
                return View("NotAccess");

            InitializeViewBag("Edit", returnUrl, canEditBaseInfo);
            return View("Claim", dataService.GetClaimViewModel(claim));
        }

        [HttpPost]
        public ActionResult Edit(ClaimVM claimVM, string returnUrl)
        {
            if (claimVM == null || claimVM.Claim == null)
                return NotFound();

            var canEditBaseInfo = securityService.HasPrivilege(Privileges.ClaimsWrite);

            if (!canEditBaseInfo)
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                dataService.Edit(claimVM.Claim);
                return RedirectToAction("Details", new { claimVM.Claim.IdClaim });
            }
            InitializeViewBag("Edit", returnUrl, canEditBaseInfo);
            return View("Claim", dataService.GetClaimViewModel(claimVM.Claim));
        }

        public ActionResult Delete(int? idClaim, string returnUrl)
        {
            if (idClaim == null)
                return NotFound();

            var claim = dataService.GetClaim(idClaim.Value);
            if (claim == null)
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return View("NotAccess");

            InitializeViewBag("Delete", returnUrl, false);

            return View("Claim", dataService.GetClaimViewModel(claim));
        }

        [HttpPost]
        public ActionResult Delete(ClaimVM claimVM)
        {
            if (claimVM == null || claimVM.Claim == null)
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return View("NotAccess");

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
        public JsonResult GetAccounts(string text, string type)
        {
            var accounts = dataService.GetAccounts(text, type);
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
                var account = dataService.GetAccountBks(idAccount);
                var rentObjects = dataService.GetRentObjectsBks(new List<int> { idAccount });
                var lastPaymentInfo = dataService.GetLastPaymentsInfo(new List<int> { idAccount }).Select(v => v.Value).FirstOrDefault();
                return Json(new
                {
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
                var account = dataService.GetAccountKumi(idAccount);
                var tenancyInfoDict = dataService.GetTenancyInfoKumi(new List<int> { idAccount }).FirstOrDefault();
                List<KumiAccountTenancyInfoVM> kumiTenancyInfo = null;
                List<KumiAccountTenancyInfoVM> activeKumiTenancyInfo = null;
                List<TenancyRentObject> rentObjects = null;
                if (tenancyInfoDict.Value != null && tenancyInfoDict.Value.Any())
                {
                    kumiTenancyInfo = tenancyInfoDict.Value;
                    activeKumiTenancyInfo = kumiTenancyInfo.Where(r => r.TenancyProcess.TenancyPersons.Any(p => p.ExcludeDate == null || p.ExcludeDate > DateTime.Now)
                        && (r.TenancyProcess.RegistrationNum == null || !r.TenancyProcess.RegistrationNum.EndsWith("н"))).ToList();
                }
                if (activeKumiTenancyInfo != null && activeKumiTenancyInfo.Any())
                {
                    kumiTenancyInfo = activeKumiTenancyInfo;
                }
                if (kumiTenancyInfo != null && kumiTenancyInfo.Any())
                {
                    rentObjects = kumiTenancyInfo.First().RentObjects;
                }

                return Json(new
                {
                    RegistryAddress = rentObjects != null && rentObjects.Any() ? rentObjects.Where(r => r.Address != null)
                        .Select(r => r.Address.Text).Aggregate((acc, v) => acc + ", " + v) : "",
                    AmountTenancy = account.CurrentBalanceTenancy,
                    AmountPenalties = account.CurrentBalancePenalty
                });
            }
            return Json(null);
        }

        [HttpPost]
        public IActionResult AddClaimState(string action, int idClaim)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Json(-2);

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
        public IActionResult AddCourtOrder(string action, int idAccount)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Json(-2);
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
        public IActionResult AddClaimFile(string action)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Json(-2);

            var file = new ClaimFile { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;

            return PartialView("AttachmentFile", file);
        }

        [HttpPost]
        public IActionResult AddClaimPerson(string action)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Json(-2);

            var file = new ClaimPerson { };
            ViewBag.SecurityService = securityService;
            ViewBag.Action = action;
            ViewBag.CanEditBaseInfo = true;

            return PartialView("ClaimPerson", file);
        }

        public IActionResult ClaimsReports(PageOptions pageOptions)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsRead))
                return View("NotAccess");

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

        public IActionResult UpdateDeptPeriod(DateTime? startDeptPeriod, DateTime? endDeptPeriod)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Error("У вас нет прав на выполнение данной операции");

            var ids = GetSessionIds();

            if (!ids.Any())
                return NotFound();

            dataService.UpdateDeptPeriodInClaims(ids, startDeptPeriod, endDeptPeriod);

            return RedirectToAction("ClaimsReports");
        }

        public IActionResult AddClaimStateMass(ClaimState claimState)
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return Error("У вас нет прав на выполнение данной операции");

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