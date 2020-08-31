using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
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
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
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

        public ActionResult Create()
        {
            if (!securityService.HasPrivilege(Privileges.ClaimsWrite))
                return View("NotAccess");
            InitializeViewBag("Create", null, true);
            return View("Claim", dataService.CreateClaimEmptyViewModel());
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
        }

        [HttpPost]
        public JsonResult GetAccounts(string text)
        {
            var accounts = dataService.GetAccounts(text);
            return Json(accounts.Select(pa => new {
                pa.IdAccount,
                pa.Account
            }));
        }

        [HttpPost]
        public JsonResult GetRentAddress(int idAccount)
        {
            var account = dataService.GetAccount(idAccount);
            var rentObjects = dataService.GetRentObjects(new List<int> { idAccount });
            return Json(new {
                BksAddress = account?.RawAddress,
                RegistryAddress = rentObjects.ContainsKey(idAccount) ? rentObjects[idAccount].Select(r => r.Text).Aggregate((acc, v) => acc + ", " + v) : ""
            });
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
    }
}