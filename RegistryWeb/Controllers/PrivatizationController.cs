using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PrivatizationController : ListController<PrivatizationDataService, PrivatizationFilter>
    {
        public PrivatizationController(PrivatizationDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
        }

        public IActionResult Index(PrivatizationListVM viewModel)
        {
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        //public IActionResult Create()
        //{
        //    canEditBaseInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
        //    canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
        //    canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
        //    if (!canEditBaseInfo)
        //        return View("NotAccess");
        //    return GetBuildingView(null);
        //}

        //[HttpPost]
        //public IActionResult Create(Building building)
        //{
        //    if (building == null)
        //        return NotFound();
        //    canEditBaseInfo = securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) || securityService.HasPrivilege(Privileges.RegistryWriteMunicipal);
        //    canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
        //    canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
        //    if (!canEditBaseInfo)
        //        return View("NotAccess");
        //    if (!canEditDemolishingInfo)
        //    {
        //        building.BuildingDemolitionActFiles = null;
        //    }
        //    if (!canAttachAdditionalFiles)
        //    {
        //        building.BuildingAttachmentFilesAssoc = null;
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        dataService.Create(building, HttpContext.Request.Form.Files.Select(f => f).ToList());
        //        return RedirectToAction("Details", new { building.IdBuilding });
        //    }
        //    return Error("Здание не было создано!");
        //}

        public IActionResult Details(int? idContract, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!idContract.HasValue)
                return NotFound();
            //if (!securityService.HasPrivilege(Privileges.RegistryRead))
            //    return View("NotAccess");
            var privContract = dataService.GetPrivContract(idContract.Value);
            if (privContract == null)
                return NotFound();
            return View("Privatization", privContract);
        }

        //[HttpGet]
        //public IActionResult Delete(int? idBuilding, string returnUrl)
        //{
        //    ViewBag.ReturnUrl = returnUrl;
        //    if (idBuilding == null)
        //        return NotFound();
        //    var building = dataService.GetBuilding(idBuilding.Value);
        //    if (building == null)
        //        return NotFound();
        //    if (!CanEditBuildingBaseInfo(building))
        //        return View("NotAccess");
        //    canEditBaseInfo = false;
        //    canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
        //    canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
        //    return GetBuildingView(building);
        //}

        //[HttpPost]
        //public IActionResult Delete(Building building)
        //{
        //    if (building == null)
        //        return NotFound();
        //    var b = dataService.GetBuilding(building.IdBuilding);
        //    if (b == null)
        //        return NotFound();
        //    canEditBaseInfo = CanEditBuildingBaseInfo(b);
        //    if (!canEditBaseInfo)
        //        return View("NotAccess");
        //    dataService.Delete(b.IdBuilding);
        //    return RedirectToAction("Index");
        //}

        //[HttpGet]
        //public IActionResult Edit(int? idBuilding, string returnUrl)
        //{
        //    ViewBag.ReturnUrl = returnUrl;
        //    if (idBuilding == null)
        //        return NotFound();
        //    var building = dataService.GetBuilding(idBuilding.Value);
        //    if (building == null)
        //        return NotFound();
        //    canEditBaseInfo = CanEditBuildingBaseInfo(building);
        //    canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
        //    canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
        //    if (!(canEditBaseInfo || canEditDemolishingInfo || canAttachAdditionalFiles))
        //        return View("NotAccess");
        //    return GetBuildingView(building);
        //}

        //[HttpPost]
        //public IActionResult Edit(Building building, string returnUrl)
        //{
        //    ViewBag.ReturnUrl = returnUrl;
        //    if (building == null)
        //        return NotFound();
        //    canEditBaseInfo = CanEditBuildingBaseInfo(building);
        //    canEditDemolishingInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
        //    canAttachAdditionalFiles = securityService.HasPrivilege(Privileges.RegistryAttachAdditionalFiles);
        //    if (!canEditBaseInfo)
        //        return View("NotAccess");
        //    if (ModelState.IsValid)
        //    {
        //        dataService.Edit(building);
        //        return RedirectToAction("Details", new { building.IdBuilding });
        //    }
        //    return Error("Здание не было сохранено!");
        //}
    }
}