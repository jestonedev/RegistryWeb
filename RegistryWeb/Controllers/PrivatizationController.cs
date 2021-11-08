using Microsoft.AspNetCore.Authorization;
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
            ViewBag.Regions = dataService.Regions;
            ViewBag.Streets = dataService.Streets;
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
                contract.IdExecutor = 65536;
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
            ViewBag.Regions = dataService.Regions;
            ViewBag.Streets = dataService.Streets;
            ViewBag.TypesOfProperty = dataService.TypesOfProperty;
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
        public JsonResult PrivContractorAdd(ActionTypeEnum action)
        {
            if (!securityService.HasPrivilege(Privileges.PrivReadWrite))
                return Json(-2);
            var contractor = new PrivContractor()
            {
                IdKinship = 64
            };
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.Formatting = Newtonsoft.Json.Formatting.Indented;
            return Json(contractor, settings);
        }

        [HttpPost]
        public IActionResult PrivContractorElemAdd(PrivContractor contractor, ActionTypeEnum action)
        {
            if (!securityService.HasPrivilege(Privileges.PrivReadWrite))
                return Json(-2);
            ViewBag.Action = action;
            ViewBag.Kinships = dataService.Kinships;
            ViewBag.Executors = dataService.Executors;
            return PartialView("PrivContractor", contractor);
        }
    }
}