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
            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));
        }

        public IActionResult Create()
        {
            return View(dataService.CreateViewModel());
        }

        [HttpPost]
        public IActionResult Create(OwnerProcessVM viewModel)
        {
            if (viewModel != null)
            {
                dataService.Create(viewModel);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult AddressAdd(int id)
        {
            return ViewComponent("AddressComponent", new { address = new Address(), id = id });
        }

        [HttpPost]
        public IActionResult OwnerReasonAdd(int id)
        {
            return ViewComponent("OwnerReasonComponent", new { ownerReason = new OwnerReasons(), id = id });
        }

        [HttpPost]
        public IActionResult OwnerAdd(int idOwnerType, int id = 0)
        {
            //физ. лицо
            if (idOwnerType == 1)
                return ViewComponent("OwnerPersonComponent", new { ownerPerson = new OwnerPersons(), id = id });
                    
            //юр. лицо или ип
            return ViewComponent("OwnerOrginfoComponent", new { ownerOrginfo = new OwnerOrginfos(), id = id });
        }

        public IActionResult Details(int? idProcess)
        {
            if (idProcess == null)
                return NotFound();
            var viewModel = dataService.GetViewModel(idProcess.Value);
            if (viewModel == null)
                return NotFound();
            return View("Details", viewModel);
        }

        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int? idProcess)
        {
            if (idProcess != null)
            {
                var ownerProcess = dataService.GetOwnerProcess(idProcess.Value);
                if (ownerProcess != null)
                    return View(ownerProcess);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Delete(int? idProcess)
        {
            if (idProcess != null)
            {
                dataService.Delete(idProcess.Value);
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        //public IActionResult Edit(int? idReasonType)
        //{
        //    if (idReasonType != null)
        //    {
        //        var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
        //        if (ownerReasonTypes != null)
        //            return View(ownerReasonTypes);
        //    }
        //    return NotFound();
        //}

        //[HttpPost]
        //public IActionResult Edit(OwnerReasonTypes ownerReasonTypes)
        //{
        //    rc.OwnerReasonTypes.Update(ownerReasonTypes);
        //    rc.SaveChanges();
        //    return RedirectToAction("Index");
        //}



        public IActionResult Error()
        {
            @ViewData["TextError"] = "Тип используется в основаниях собственности!";
            ViewData["Controller"] = "OwnerProcesses";
            return View("Error");
        }
    }
}