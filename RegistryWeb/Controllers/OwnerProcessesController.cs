using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    public class OwnerProcessesController : ListController<OwnerProcessesDataService>
    {
        public OwnerProcessesController(OwnerProcessesDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
        }

        public IActionResult Index(OwnerProcessesVM viewModel)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            ViewBag.OwnerTypes = dataService.GetOwnerTypes;
            ViewBag.SecurityService = securityService;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        public IActionResult Create()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            return GetOwnerProcessView(dataService.CreateOwnerProcess());
        }

        [HttpPost]
        public IActionResult Create(OwnerProcess ownerProcess)
        {
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Create(ownerProcess);
                return RedirectToAction("Index");
            }
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpPost]
        public IActionResult AddressAdd(int id, string action)
        {
            return ViewComponent("AddressComponent", new { addressAssoc = new OwnerBuildingAssoc(), id, action });
        }

        [HttpPost]
        public IActionResult OwnerReasonAdd(int id, string action)
        {
            return ViewComponent("OwnerReasonComponent", new { ownerReason = new OwnerReason(), id, action });
        }

        [HttpPost]
        public IActionResult OwnerAdd(int idOwnerType, int id, string action)
        {
            //физ. лицо
            if (idOwnerType == 1)
                return ViewComponent("OwnerPersonComponent", new { ownerPerson = new OwnerPerson(), id, action });

            //юр. лицо или ип
            return ViewComponent("OwnerOrginfoComponent", new { ownerOrginfo = new OwnerOrginfo(), id, action });
        }

        public IActionResult Details(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpGet]
        public IActionResult Delete(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpPost]
        public IActionResult Delete(OwnerProcess ownerProcess)
        {
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            dataService.Delete(ownerProcess.IdProcess);
            return RedirectToAction("Index");            
        }

        [HttpGet]
        public IActionResult Edit(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpPost]
        public IActionResult Edit(OwnerProcess ownerProcess)
        {
            if (ownerProcess == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Edit(ownerProcess);
                return RedirectToAction("Index");
            }
            return GetOwnerProcessView(ownerProcess);
            
        }

        public IActionResult GetOwnerProcessView(OwnerProcess ownerProcess, [CallerMemberName]string action = "")
        {
            ViewBag.Action = action;
            ViewBag.OwnerTypes = dataService.GetOwnerTypes;
            return View("OwnerProcess", ownerProcess);
        }

        public IActionResult Error()
        {
            //ViewData["TextError"] = "Тип используется в основаниях собственности!";
            ViewData["Controller"] = "OwnerProcesses";
            return View("Error");
        }
    }
}