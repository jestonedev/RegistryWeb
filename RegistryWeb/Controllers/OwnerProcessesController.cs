﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            return GetOwnerProcessView(dataService.CreateOwnerProcess());
        }

        [HttpPost]
        public IActionResult Create(OwnerProcesses ownerProcess)
        {
            if (ownerProcess != null)
            {
                if (ModelState.IsValid)
                {
                    dataService.Create(ownerProcess);
                    return RedirectToAction("Index");
                }
                return GetOwnerProcessView(ownerProcess);
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
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpGet]
        public IActionResult ConfirmDelete(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess);
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

        [HttpGet]
        public IActionResult Edit(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
            if (ownerProcess == null)
                return NotFound();
            return GetOwnerProcessView(ownerProcess);
        }

        [HttpPost]
        public IActionResult Edit(OwnerProcesses ownerProcess)
        {
            if (ownerProcess != null)
            {
                if (ModelState.IsValid)
                {
                    dataService.Edit(ownerProcess);
                    return RedirectToAction("Index");
                }
                return GetOwnerProcessView(ownerProcess);
            }
            return NotFound();
        }

        public IActionResult GetOwnerProcessView(OwnerProcesses ownerProcess, [CallerMemberName]string action = "")
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