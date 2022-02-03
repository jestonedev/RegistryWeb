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
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
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
    }
}