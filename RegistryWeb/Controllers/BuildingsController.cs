﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.DataHelpers;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryDb.Models.Entities;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.RegistryObjects;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class BuildingsController : ListController<BuildingsDataService, BuildingsFilter>
    {
        OwnerReportService reportService;
        bool canEditBaseInfo;
        bool canEditDemolishingInfo;
        bool canAttachAdditionalFiles;
        bool canEditLandInfo;

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
            ViewBag.SecurityService = securityService;
            ViewBag.SignersList = new SelectList(dataService.SelectableSigners.Select(s => new {
                s.IdRecord,
                Snp = s.Surname + " " + s.Name + (s.Patronymic == null ? "" : " " + s.Patronymic)
            }), "IdRecord", "Snp");
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
            canEditBaseInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
            canEditLandInfo = canEditBaseInfo;
            canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
            canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
            if (!canEditBaseInfo)
                return View("NotAccess");
            return GetBuildingView(null);
        }

        [HttpPost]
        public IActionResult Create(Building building)
        {
            if (building == null)
                return NotFound();
            canEditBaseInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
            canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
            canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
            if (!canEditBaseInfo)
                return View("NotAccess");
            if (!canEditDemolishingInfo)
            {
                building.BuildingDemolitionActFiles = null;
            }
            if (!canAttachAdditionalFiles)
            {
                building.BuildingAttachmentFilesAssoc = null;
            }
            if (ModelState.IsValid)
            {
                dataService.Create(building, HttpContext.Request.Form.Files.Select(f => f).ToList());
                return RedirectToAction("Details", new { building.IdBuilding });
            }
            return Error("Здание не было создано!");
        }

        public IActionResult Details(int? idBuilding, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (idBuilding == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            var building = dataService.GetBuilding(idBuilding.Value);
            if (building == null)
                return NotFound();
            canEditBaseInfo = CanEditBuildingBaseInfo(building);
            canEditLandInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
            canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
            canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
            return GetBuildingView(building);
        }

        [HttpGet]
        public IActionResult Delete(int? idBuilding, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (idBuilding == null)
                return NotFound();
            var building = dataService.GetBuilding(idBuilding.Value);
            if (building == null)
                return NotFound();
            if (!CanEditBuildingBaseInfo(building))
                return View("NotAccess");
            canEditBaseInfo = false;
            canEditLandInfo = false;
            canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
            canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
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
            canEditBaseInfo = CanEditBuildingBaseInfo(b);
            if (!canEditBaseInfo)
                return View("NotAccess");
            dataService.Delete(b.IdBuilding);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int? idBuilding, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (idBuilding == null)
                return NotFound();
            var building = dataService.GetBuilding(idBuilding.Value);
            if (building == null)
                return NotFound();
            canEditBaseInfo = CanEditBuildingBaseInfo(building);
            canEditLandInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
            canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
            canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
            if (!(canEditBaseInfo || canEditDemolishingInfo || canAttachAdditionalFiles))
                return View("NotAccess");
            return GetBuildingView(building);
        }

        [HttpPost]
        public IActionResult Edit(Building building, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (building == null)
                return NotFound();
            canEditBaseInfo = CanEditBuildingBaseInfo(building);
            canEditLandInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
            canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
            canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
            if (!canEditBaseInfo && !canEditLandInfo)
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Edit(building, canEditBaseInfo, canEditLandInfo);
                return RedirectToAction("Details", new { building.IdBuilding });
            }
            return Error("Здание не было сохранено!");
        }

        private bool CanEditBuildingBaseInfo(Building building)
        {
            return (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !dataService.IsMunicipal(building)) ||
                (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && dataService.IsMunicipal(building));
        }

        public IActionResult GetBuildingView(Building building, [CallerMemberName]string action = "")
        {
            var actionType = (ActionTypeEnum)Enum.Parse(typeof(ActionTypeEnum), action);
            ViewBag.Action = actionType;
            ViewBag.SecurityService = securityService;
            ViewBag.CanEditBaseInfo = canEditBaseInfo;
            ViewBag.CanEditLandInfo = canEditLandInfo;
            ViewBag.CanEditDemolishingInfo = canEditDemolishingInfo;
            ViewBag.CanAttachAdditionalFiles = canAttachAdditionalFiles;
            ViewBag.ObjectStates = dataService.GetObjectStates(securityService, action, canEditBaseInfo);
            ViewBag.StructureTypes = dataService.StructureTypes;
            ViewBag.StructureTypeOverlaps = dataService.StructureTypeOverlaps;
            ViewBag.KladrStreets = dataService.KladrStreets;
            ViewBag.HeatingTypes = dataService.HeatingTypes;
            ViewBag.GovernmentDecrees = dataService.GovernmentDecrees;
            ViewBag.FoundationTypes = dataService.FoundationTypes;
            ViewBag.BuildingManagmentOrgs = dataService.BuildingManagmentOrgs;
            ViewBag.SignersList = new SelectList(dataService.SelectableSigners.Select(s => new {
                s.IdRecord,
                Snp = s.Surname + " " + s.Name + (s.Patronymic == null ? "" : " " + s.Patronymic)
            }), "IdRecord", "Snp");
            if (actionType == ActionTypeEnum.Create)
            {
                return View("Building", dataService.CreateBuilding());
            }
            ViewBag.Address = building.IdStreetNavigation?.StreetName + ", д." + building.House;
            ViewBag.PremisesAreaInfo = new PremisesAreaInfo
            {
                MunicipalLivingArea = dataService.GetPremisesLivingAreaByStates(building.IdBuilding, ObjectStateHelper.MunicipalIds()),
                MunicipalTotalArea = dataService.GetPremisesTotalAreaByStates(building.IdBuilding, ObjectStateHelper.MunicipalIds()),
                PrivLivingArea = dataService.GetPremisesLivingAreaByStates(building.IdBuilding, ObjectStateHelper.MunicipalIds(), false),
                PrivTotalArea = dataService.GetPremisesLivingAreaByStates(building.IdBuilding, ObjectStateHelper.MunicipalIds(), false)
            };
            return View("Building", building);
        }
    }
}
