using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    public class OwnerProcessesController : ListController<OwnerProcessesDataService>
    {
        public OwnerProcessesController(OwnerProcessesDataService dataService) : base(dataService)
        {
        }

        public IActionResult Index(OwnerProcessesListVM viewModel)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            ViewBag.OwnerTypes = dataService.GetOwnerTypes;
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        public IActionResult Create()
        {
            ViewBag.Action = "Create";
            ViewBag.OwnerTypes = dataService.GetOwnerTypes;
            return View("OwnerProcess", dataService.CreateOwnerProcess());
        }

        [HttpPost]
        public IActionResult Create(OwnerProcesses ownerProcess)
        {
            if (ownerProcess != null)
            {
                dataService.Create(ownerProcess);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult AddressAdd(int id, string action)
        {
            return ViewComponent("AddressComponent", new { addressAssoc = new OwnerBuildingsAssoc(), id, action });
        }

        [HttpPost]
        public IActionResult OwnerReasonAdd(int id, string action)
        {
            return ViewComponent("OwnerReasonComponent", new { ownerReason = new OwnerReasons(), id, action });
        }

        [HttpPost]
        public IActionResult OwnerAdd(int idOwnerType, int id, string action)
        {
            //физ. лицо
            if (idOwnerType == 1)
                return ViewComponent("OwnerPersonComponent", new { ownerPerson = new OwnerPersons(), id, action });
                    
            //юр. лицо или ип
            return ViewComponent("OwnerOrginfoComponent", new { ownerOrginfo = new OwnerOrginfos(), id, action });
        }

        public IActionResult Details(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            ViewBag.Action = "Details";
            ViewBag.OwnerTypes = dataService.GetOwnerTypes;
            return View("OwnerProcess", ownerProcess);
        }

        [HttpGet, ActionName("Delete")]
        public IActionResult ConfirmDelete(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            ViewBag.Action = "Delete";
            ViewBag.OwnerTypes = dataService.GetOwnerTypes;
            return View("OwnerProcess", ownerProcess);
        }

        [HttpPost]
        public IActionResult Delete(OwnerProcesses ownerProcess)
        {
            if (ownerProcess != null)
            {
                dataService.Delete(ownerProcess.IdProcess);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        [HttpGet, ActionName("Edit")]
        public IActionResult ConfirmEdit(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            ViewBag.Action = "Edit";
            ViewBag.OwnerTypes = dataService.GetOwnerTypes;
            return View("OwnerProcess", ownerProcess);
        }

        [HttpPost]
        public IActionResult Edit(OwnerProcesses ownerProcess)
        {
            if (ownerProcess != null)
            {
                dataService.Edit(ownerProcess);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public IActionResult Error()
        {
            //ViewData["TextError"] = "Тип используется в основаниях собственности!";
            ViewData["Controller"] = "OwnerProcesses";
            return View("Error");
        }
    }
}