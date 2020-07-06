﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.Models.Entities;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class BuildingsController : ListController<BuildingsDataService>
    {
        OwnerReportService reportService;

        public BuildingsController(BuildingsDataService dataService, SecurityService securityService, OwnerReportService reportService)
            : base(dataService, securityService)
        {
            this.reportService = reportService;
        }

        public IActionResult Index(BuildingsVM viewModel, bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<BuildingsFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }
            ViewBag.ObjectStates = dataService.ObjectStates;
            ViewBag.KladrStreets = dataService.KladrStreets;
            ViewBag.OwnershipRightTypes = dataService.OwnershipRightTypes;
            ViewBag.GovernmentDecrees = dataService.GovernmentDecrees;
            ViewBag.SecurityService = securityService;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        public IActionResult BuildingReports()
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (HttpContext.Session.Keys.Contains("idBuildings"))
            {
                var ids = HttpContext.Session.Get<List<int>>("idBuildings");
                if (ids.Any())
                {
                    var buildings = dataService.GetBuildings(ids);
                    return View("BuildingReports", buildings);
                }
            }
            return View("BuildingReports", new List<Building>());
        }

        public IActionResult Forma1()
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (!HttpContext.Session.Keys.Contains("idBuildings"))
                return Error("Не выбрано ни одного здания.");
            var ids = HttpContext.Session.Get<List<int>>("idBuildings");
            if (!ids.Any())
                return Error("Не выбрано ни одного здания.");
            try
            {
                var file = reportService.Forma1(ids);
                return File(file, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    @"Форма 1. Общие сведения об аварийном многоквартирном доме г. Братск.docx");
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        [HttpPost]
        public void SessionIdBuildings(int idBuilding, bool isCheck)
        {
            List<int> ids;
            if (HttpContext.Session.Keys.Contains("idBuildings"))
            {
                ids = HttpContext.Session.Get<List<int>>("idBuildings");
            }
            else
            {
                ids = new List<int>();
            }
            if (isCheck)
            {
                ids.Add(idBuilding);
            }
            else if(ids.Any())
            {
                ids.Remove(idBuilding);
            }
            HttpContext.Session.Set("idBuildings", ids);
        }

        public IActionResult SessionIdBuildingsClear()
        {
            HttpContext.Session.Remove("idBuildings");
            return BuildingReports();
        }

        public IActionResult SessionIdBuildingRemove(int idBuilding)
        {
            var ids = HttpContext.Session.Get<List<int>>("idBuildings");
            ids.Remove(idBuilding);
            HttpContext.Session.Set("idBuildings", ids);
            return BuildingReports();
        }

        public IActionResult Create()
        {
            ViewBag.CanEditBaseInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
            if (!ViewBag.CanEditBaseInfo)
                return View("NotAccess");
            return GetBuildingView(dataService.CreateBuilding());
        }

        [HttpPost]
        public IActionResult Create(Building building)
        {
            if (building == null)
                return Json(-1);
            ViewBag.CanEditBaseInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
            if (!ViewBag.CanEditBaseInfo)
                return Json(-2);
            if (ModelState.IsValid)
            {
                dataService.Create(building);
                return Json(building.IdBuilding);
            }
            return Json(0);
        }

        public IActionResult Details(int? idBuilding)
        {
            if (idBuilding == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            var building = dataService.GetBuilding(idBuilding.Value);
            if (building == null)
                return NotFound();
            ViewBag.CanEditBaseInfo = false;
            return GetBuildingView(building);
        }

        [HttpGet]
        public IActionResult Delete(int? idBuilding)
        {
            if (idBuilding == null)
                return NotFound();
            var building = dataService.GetBuilding(idBuilding.Value);
            if (building == null)
                return NotFound();
            ViewBag.CanEditBaseInfo = (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !dataService.IsMunicipal(building)) ||
                (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && dataService.IsMunicipal(building));
            if (!ViewBag.CanEditBaseInfo)
                return View("NotAccess");
            return GetBuildingView(building);
        }

        [HttpPost]
        public IActionResult Delete(Building building)
        {
            if (building == null)
                return NotFound();
            var b = dataService.GetBuilding(building.IdBuilding);
            if (b == null)
                return NotFound();
            ViewBag.CanEditBaseInfo = (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !dataService.IsMunicipal(b)) ||
                (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && dataService.IsMunicipal(b));
            if (!ViewBag.CanEditBaseInfo)
                return View("NotAccess");
            dataService.Delete(b.IdBuilding);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int? idBuilding)
        {
            if (idBuilding == null)
                return NotFound();
            var building = dataService.GetBuilding(idBuilding.Value);
            if (building == null)
                return NotFound();
            ViewBag.CanEditBaseInfo = (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !dataService.IsMunicipal(building)) ||
                (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && dataService.IsMunicipal(building));
            var canEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);
            if (!(ViewBag.CanEditBaseInfo || canEditExtInfo))
                return View("NotAccess");
            return GetBuildingView(building);
        }

        [HttpPost]
        public IActionResult Edit(Building building)
        {
            if (building == null)
                return NotFound();
            ViewBag.CanEditBaseInfo = (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !dataService.IsMunicipal(building)) ||
                (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && dataService.IsMunicipal(building));
            if (!ViewBag.CanEditBaseInfo)
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Edit(building);
                return RedirectToAction("Index");
            }
            return GetBuildingView(building);
        }

        public IActionResult GetBuildingView(Building building, [CallerMemberName]string action = "")
        {
            ViewBag.Action = action;
            ViewBag.ObjectStates = dataService.ObjectStates;
            ViewBag.StructureTypes = dataService.StructureTypes;
            ViewBag.StructureTypeOverlaps = dataService.StructureTypeOverlaps;
            ViewBag.KladrStreets = dataService.KladrStreets;
            ViewBag.HeatingTypes = dataService.HeatingTypes;
            ViewBag.GovernmentDecrees = dataService.GovernmentDecrees;
            return View("Building", building);
        }
    }
}
