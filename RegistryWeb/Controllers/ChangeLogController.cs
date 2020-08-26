﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class ChangeLogController : ListController<ChangeLogsDataService, ChangeLogsFilter>
    {
        public ChangeLogController(ChangeLogsDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
        }

        public IActionResult Index(ChangeLogsVM viewModel)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }
    }
}