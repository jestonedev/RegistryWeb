using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Extensions;
using RegistryWeb.ViewOptions;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Controllers
{
    public class KumiAccountsController :  ListController<KumiAccountsDataService, KumiAccountsFilter>
    {
        public KumiAccountsController(KumiAccountsDataService dataService, SecurityService securityService)
            :base(dataService, securityService)
        {
            nameFilteredIdsDict = "filteredKumiAccountsIdsDict";
            nameIds = "idKumiAccounts";
            nameMultimaster = "KumiAccountsReports";
        }

        public IActionResult Index(KumiAccountsVM viewModel, bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.AccountsRead))
                return View("NotAccess");
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<KumiAccountsFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
            ViewBag.SecurityService = securityService;

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions, out List<int> filteredTenancyProcessesIds);

            AddSearchIdsToSession(vm.FilterOptions, filteredTenancyProcessesIds);

            return View(vm);
        }

        private IActionResult GetView(int? idAccount, string returnUrl, ActionTypeEnum action, Privileges privilege)
        {
            if (!securityService.HasPrivilege(privilege))
                return View("NotAccess");
            KumiAccount account = new KumiAccount();
            if (action != ActionTypeEnum.Create)
            {
                if (!idAccount.HasValue)
                    return NotFound();
                account = dataService.GetKumiAccount(idAccount.Value);
                if (account == null)
                    return NotFound();
            }
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Action = action;
            ViewBag.SecurityService = securityService;
            ViewBag.States = dataService.States;
            return View("Account", account);
        }

        [HttpGet]
        public IActionResult Create(string returnUrl)
        {
            return GetView(null, returnUrl, ActionTypeEnum.Create, Privileges.AccountsReadWrite);
        }

        [HttpGet]
        public IActionResult Details(int? idAccount, string returnUrl)
        {
            return GetView(idAccount, returnUrl, ActionTypeEnum.Details, Privileges.AccountsRead);
        }

        [HttpGet]
        public IActionResult Edit(int? idAccount, string returnUrl)
        {
            return GetView(idAccount, returnUrl, ActionTypeEnum.Edit, Privileges.AccountsReadWrite);
        }

        [HttpGet]
        public IActionResult Delete(int? idAccount, string returnUrl)
        {
            return GetView(idAccount, returnUrl, ActionTypeEnum.Delete, Privileges.AccountsReadWrite);
        }

        [HttpPost]
        public IActionResult Create(KumiAccount kumiAccount)
        {
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite);
            if (!canEdit)
                return View("NotAccess");
            if (kumiAccount == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create(kumiAccount);
                return RedirectToAction("Details", new { kumiAccount.IdAccount });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(KumiAccount kumiAccount)
        {
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite);
            if (!canEdit)
                return View("NotAccess");
            if (kumiAccount == null)
                return NotFound();
            dataService.Delete(kumiAccount.IdAccount);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(KumiAccount kumiAccount)
        {
            var canEdit = securityService.HasPrivilege(Privileges.AccountsReadWrite);
            if (!canEdit)
                return View("NotAccess");
            if (kumiAccount == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Edit(kumiAccount);
                return RedirectToAction("Details", new { kumiAccount.IdAccount });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AccountExist(string account, int idAccount)
        {
            if (account == null)
                return Json(false);
            var isExist = dataService.AccountExists(account, idAccount);
            return Json(isExist);
        }
    }
}