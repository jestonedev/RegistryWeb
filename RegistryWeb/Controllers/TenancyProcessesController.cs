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

namespace RegistryWeb.Controllers
{
    public class TenancyProcessesController : ListController<TenancyProcessesDataService>
    {
        public TenancyProcessesController(TenancyProcessesDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
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
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        // GET: TenancyProcesses/Details/5
        public ActionResult Details(int? idProcess, string action = "")
        {
            ViewBag.Action = action;
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.TenancyRead))
                return View("NotAccess");
            var process = dataService.GetTenancyProcess(idProcess.Value);
            if (process == null)
                return NotFound();

            return View("TenancyProcess", dataService.GetTenancyProcessViewModel(process));
        }

        // GET: TenancyProcesses/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TenancyProcesses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TenancyProcesses/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TenancyProcesses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TenancyProcesses/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TenancyProcesses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}