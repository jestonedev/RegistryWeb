using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Runtime.CompilerServices;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using RegistryWeb.Models;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PremisesController : ListController<PremisesDataService>
    {
        private readonly RegistryContext rc;
        public PremisesController(RegistryContext rc, PremisesDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
            this.rc = rc;
        }

        public IActionResult Index(PremisesVM<Premise> viewModel, string action="", bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<PremisesListFilter>("FilterOptions");
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

        public IActionResult Details(int? idPremises)
        {
            ViewBag.Action = "Details";
            if (idPremises == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();
            //return dataService.GetPremiseView(premise);

            return View("Premise", dataService.GetPremiseView(premise));
        }

        public IActionResult Create()
        {
            ViewBag.Action = "Create";
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            return View("Premise", dataService.GetPremiseView(dataService.CreatePremise()));
        }

        [HttpPost]
        public IActionResult Create(Premise premise, int? IdFundType)
        {
            if (premise == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal))
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                dataService.Create(premise, IdFundType);
                return RedirectToAction("Details", new { premise.IdPremises });
            }
            ViewBag.Action = "Create";
            return View("Premise", dataService.GetPremiseView(premise));
        }

        [HttpGet]
        public IActionResult Edit(int? idPremises)
        {
            ViewBag.Action = "Edit";
            if (idPremises == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal))
                return View("NotAccess");

            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();

            return View("Premise", dataService.GetPremiseView(premise));
        }

        [HttpPost]
        public IActionResult Edit(Premise premise)
        {
            if (premise == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Edit(premise);
                return RedirectToAction("Details", new { premise.IdPremises });
            }
            ViewBag.Action = "Edit";
            return View("Premise", dataService.GetPremiseView(dataService.CreatePremise()));
        }

        [HttpGet]
        public IActionResult Delete(int? idPremises)
        {
            ViewBag.Action = "Delete";
            if (idPremises == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal))
                return View("NotAccess");
            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();

            return View("Premise", dataService.GetPremiseView(premise));
        }

        [HttpPost]
        public IActionResult Delete(Premise premise)
        {
            if (premise == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal))
                return View("NotAccess");
            dataService.Delete(premise.IdPremises);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RestrictionAdd(int id, AddressTypes type, string action)
        {
            var restriction = new Restriction();
            restriction.RestrictionTypeNavigation = new RestrictionType();
            restriction.RestrictionPremisesAssoc = new List<RestrictionPremiseAssoc>() { new RestrictionPremiseAssoc() };
            restriction.RestrictionBuildingsAssoc = new List<RestrictionBuildingAssoc>() { new RestrictionBuildingAssoc() };
            return ViewComponent("RestrictionsComponent", new { id, type, action });
        }


        public JsonResult GetHouse(string streetId)
        {
            IEnumerable<Building> buildings = dataService.GetHouses(streetId);
            return Json(buildings);
        }


    }
}
