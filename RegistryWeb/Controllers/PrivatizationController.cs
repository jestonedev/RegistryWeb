using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
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

        private IActionResult GetView(int? idContract, string returnUrl, ActionTypeEnum action, Privileges privilege)
        {
            if (!securityService.HasPrivilege(privilege))
                    return View("NotAccess");
            PrivContract contract = null;
            if (action != ActionTypeEnum.Create)
            {
                if (!idContract.HasValue)
                    return NotFound();
                contract = dataService.GetPrivContract(idContract.Value);
                if (contract == null)
                    return NotFound();
            }
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Action = action;
            ViewBag.Kinships = dataService.Kinships;
            return View("Privatization", contract);
        }

        [HttpGet]
        public IActionResult Create(string returnUrl)
        {
            return GetView(null, returnUrl, ActionTypeEnum.Create, Privileges.PrivReadWrite);
        }

        [HttpGet]
        public IActionResult Details(int? idContract, string returnUrl)
        {
            return GetView(idContract, returnUrl, ActionTypeEnum.Details, Privileges.PrivRead);
        }

        [HttpGet]
        public IActionResult Edit(int? idContract, string returnUrl)
        {
            return GetView(idContract, returnUrl, ActionTypeEnum.Edit, Privileges.PrivReadWrite);
        }

        [HttpGet]
        public IActionResult Delete(int? idContract, string returnUrl)
        {
            return GetView(idContract, returnUrl, ActionTypeEnum.Delete, Privileges.PrivReadWrite);
        }

        [HttpPost]
        public IActionResult Create(PrivContract contract)
        {
            var canEdit = securityService.HasPrivilege(Privileges.PrivReadWrite);
            if (canEdit)
                return View("NotAccess");
            if (contract == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create(contract);
                return RedirectToAction("Details", new { contract.IdContract });
            }
            return Error("Договор приватизации не был создан!");
        }

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