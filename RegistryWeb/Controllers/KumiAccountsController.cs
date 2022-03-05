using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Extensions;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryWeb.Controllers
{
    public class KumiAccountsController :  ListController<KumiAccountsDataService, KumiAccountsFilter>
    {
        private TenancyProcessesDataService tenancyProcessesDataService { get; }

        public KumiAccountsController(KumiAccountsDataService dataService, TenancyProcessesDataService tenancyProcessesDataService, SecurityService securityService)
            :base(dataService, securityService)
        {
            nameFilteredIdsDict = "filteredKumiAccountsIdsDict";
            nameIds = "idKumiAccounts";
            nameMultimaster = "KumiAccountsReports";
            this.tenancyProcessesDataService = tenancyProcessesDataService;
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
            ViewBag.Streets = dataService.Streets;
            ViewBag.Regions = dataService.Regions;
            var tenancyInfo = dataService.GetTenancyInfo(new List<KumiAccount> { account });
            if (tenancyInfo.ContainsKey(account.IdAccount))
                ViewBag.TenancyInfo = tenancyInfo[account.IdAccount];
            else
                ViewBag.TenancyInfo = new List<KumiAccountTenancyInfoVM>();
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

        [HttpPost]
        public IActionResult GetTenancyInfo(TenancyProcessesFilter filterOptions)
        {
            var tenancies = tenancyProcessesDataService.GetTenancyProcesses(filterOptions);
            var count = tenancies.Count();
            var tenanciesLimit = tenancies.Take(5).ToList();
            var rentObjects = tenancyProcessesDataService.GetRentObjects(tenanciesLimit);
            return Json(new {
                Count = count,
                Tenancies = tenanciesLimit.Select(r => new {
                    r.IdProcess,
                    r.RegistrationDate,
                    r.RegistrationNum,
                    r.IdAccount,
                    r.IdAccountNavigation?.Account
                }),
                RentObjects = rentObjects
            });
        }
    }
}