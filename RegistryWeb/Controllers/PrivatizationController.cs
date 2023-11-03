using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.Privatization;
using RegistryDb.Models.Entities.Privatization;
using RegistryWeb.Filters;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.PrivRead)]
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
            ViewBag.PrivEstateOwners = dataService.PrivEstateOwners;
            ViewBag.PrivEstateOwnerSigners = dataService.PrivEstateOwnerSigners;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        private IActionResult GetView(int? idContract, string returnUrl, ActionTypeEnum action)
        {
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
            var addressesRegistry = dataService.GetContractAddresses(contract);
            ViewBag.AddressesRegistry = addressesRegistry;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Action = action;
            ViewBag.SecurityService = securityService;
            ViewBag.Kinships = dataService.Kinships;
            ViewBag.Executors = dataService.Executors;
            ViewBag.Regions = dataService.Regions;
            ViewBag.Streets = dataService.Streets;
            ViewBag.TypesOfProperty = dataService.TypesOfProperty;
            ViewBag.PrivEstateOwners = dataService.PrivEstateOwners;
            ViewBag.PrivEstateOwnerSigners = dataService.PrivEstateOwnerSigners;
            ViewBag.PrivRealtors = dataService.PrivRealtors;
            ViewBag.DocumentsIssuedBy = dataService.DocumentsIssuedBy;
            ViewBag.PrivContractorWarrantTemplates = dataService.PrivContractorWarrantTemplates;
            return View("Privatization", contract);
        }

        [HttpGet]
        [HasPrivileges(Privileges.PrivReadWrite)]
        public IActionResult Create(string returnUrl)
        {
            return GetView(null, returnUrl, ActionTypeEnum.Create);
        }

        [HttpGet]
        public IActionResult Details(int? idContract, string returnUrl)
        {
            return GetView(idContract, returnUrl, ActionTypeEnum.Details);
        }

        [HttpGet]
        [HasPrivileges(Privileges.PrivReadWrite)]
        public IActionResult Edit(int? idContract, string returnUrl)
        {
            return GetView(idContract, returnUrl, ActionTypeEnum.Edit);
        }

        [HttpGet]
        [HasPrivileges(Privileges.PrivReadWrite)]
        public IActionResult Delete(int? idContract, string returnUrl)
        {
            return GetView(idContract, returnUrl, ActionTypeEnum.Delete);
        }

        [HttpPost]
        [HasPrivileges(Privileges.PrivReadWrite)]
        public IActionResult Create(PrivContract contract)
        {
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
        [HasPrivileges(Privileges.PrivReadWrite)]
        public IActionResult Delete(PrivContract contract)
        {
            if (contract == null)
                return NotFound();
            dataService.Delete(contract.IdContract);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [HasPrivileges(Privileges.PrivReadWrite)]
        public IActionResult Edit(PrivContract contract)
        {
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
        [HasPrivileges(Privileges.PrivReadWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public JsonResult PrivContractorAdd(ActionTypeEnum action)
        {
            var contractor = new PrivContractor()
            {
                IdKinship = 64
            };
            var settings = new Newtonsoft.Json.JsonSerializerSettings();
            settings.Formatting = Newtonsoft.Json.Formatting.Indented;
            return Json(contractor, settings);
        }

        [HttpPost]
        [HasPrivileges(Privileges.PrivReadWrite, Model = -2, ViewResultType = typeof(JsonResult))]
        public IActionResult PrivContractorElemAdd(PrivContractor contractor, ActionTypeEnum action)
        {
            ViewBag.Action = action;
            ViewBag.Kinships = dataService.Kinships;
            ViewBag.Executors = dataService.Executors;
            return PartialView("PrivContractor", contractor);
        }
    }
}