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

        public IActionResult Details(int? idPremises, string action="")
        {
            ViewBag.Action = action;
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

        public IActionResult Create(string action = "")
        {
            ViewBag.Action = action;
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            return View("Premise", dataService.GetPremiseView(dataService.CreatePremise()));
        }

        [HttpPost]
        public IActionResult Create(Premise premise, int IdFundType)
        {
            if (premise == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                var bui = rc.Buildings
                        .FirstOrDefault(b => b.IdStreet == premise.IdBuildingNavigation.IdStreetNavigation.IdStreet && b.House == premise.IdBuildingNavigation.House);
                if (bui != null)
                {
                    premise.IdBuilding = bui.IdBuilding;
                    dataService.Create(premise, IdFundType);
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "На указанном адресе отсутствует здание!";
                    ViewBag.Action = "Create";
                    //return RedirectToAction("Create");
                    return View("Premise", dataService.GetPremiseView(premise));
                }
            }
            return View("Premise", dataService.GetPremiseView(premise));
        }

        [HttpGet]
        public IActionResult Edit(int? idPremises, string action = "")
        {
            ViewBag.Action = action;
            if (idPremises == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
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
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Edit(premise);
                return RedirectToAction("Index");
            }
            return View("Premise", dataService.GetPremiseView(dataService.CreatePremise()));
        }

        [HttpGet]
        public IActionResult Delete(int? idPremises, string action = "")
        {
            ViewBag.Action = action;
            if (idPremises == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
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
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Delete(premise.IdPremises);
                return RedirectToAction("Index");
            }
            return View("Premise", dataService.GetPremiseView(premise));
        }



        /*public JsonResult GetPaymentForPremiseInfo(int id)
        {
            IEnumerable<TenancyPaymentsInfo> payment = dataService.GetPaymentInfo(id);
            return Json(payment);
        }*/

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
