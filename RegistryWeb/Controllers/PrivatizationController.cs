﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
using System;

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
            ViewBag.SecurityService = securityService;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        private IActionResult GetView(int? idContract, string returnUrl, ActionTypeEnum action, Privileges privilege)
        {
            if (!securityService.HasPrivilege(privilege))
                    return View("NotAccess");
            PrivContract contract = new PrivContract();
            if (action != ActionTypeEnum.Create)
            {
                if (!idContract.HasValue)
                    return NotFound();
                contract = dataService.GetPrivContract(idContract.Value);
                if (contract == null)
                    return NotFound();
            }
            else
            {
                contract.User = securityService.User.UserName;
            }
            var addressRegistry = dataService.GetAddressRegistry(contract);
            ViewBag.AddressRegistry = addressRegistry?.Text;            
            ViewBag.IdPremise = addressRegistry?.IdParents["IdPremise"];
            ViewBag.IdBuilding = addressRegistry?.IdParents["IdBuilding"];
            ViewBag.IdStreet = addressRegistry?.IdParents["IdStreet"];
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Action = action;
            ViewBag.SecurityService = securityService;
            ViewBag.Kinships = dataService.Kinships;
            ViewBag.Executors = dataService.Executors;
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
            if (!canEdit)
                return View("NotAccess");
            if (contract == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create(contract);
                return RedirectToAction("Details", new { contract.IdContract });
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(PrivContract contract)
        {
            var canEdit = securityService.HasPrivilege(Privileges.PrivReadWrite);
            if (!canEdit)
                return View("NotAccess");
            if (contract == null)
                return NotFound();
            dataService.Delete(contract.IdContract);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Edit(PrivContract contract)
        {
            var canEdit = securityService.HasPrivilege(Privileges.PrivReadWrite);
            if (!canEdit)
                return View("NotAccess");
            if (contract == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Edit(contract);
                return RedirectToAction("Details", new { contract.IdContract });
            }
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult PrivContractorAdd(ActionTypeEnum action)
        {
            if (!securityService.HasPrivilege(Privileges.PrivReadWrite))
                return Json(-2);

            var contractor = new PrivContractor()
            {
                IdKinship = 64,
                User = securityService.User.UserName,
                InsertDate = DateTime.Now
            };
            ViewBag.Action = action;
            ViewBag.Kinships = dataService.Kinships;
            ViewBag.Executors = dataService.Executors;
            return PartialView("PrivContractor", contractor);
        }
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