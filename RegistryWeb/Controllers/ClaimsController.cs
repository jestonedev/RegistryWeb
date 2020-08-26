using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.Models;
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
                dataService.Create(claimVM.Claim);
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
            return Json(dataService.GetAccounts(text).Select(pa => new { pa.IdAccount, pa.Account }));
        }
    }
}