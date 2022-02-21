using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataServices;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PrivRealtorsController : RegistryBaseController
    {
        private readonly RegistryContext rc;
        private readonly SecurityService securityService;
        private readonly PrivRealtorService dataService;

        public PrivRealtorsController(RegistryContext rc, SecurityService securityService, PrivRealtorService dataService)
        {
            this.rc = rc;
            this.securityService = securityService;
            this.dataService = dataService;
        }

        public IActionResult Index(PageOptions pageOptions)
        {
            return View(dataService.GetViewModel(pageOptions));
        }

        public IActionResult Create()
        {
            ViewBag.Action = "Create";
            return View("PrivRealtor", dataService.GetPrivRealtorView(new PrivRealtor()));
        }

        [HttpPost]
        public IActionResult Create(PrivRealtor privRealtor)
        {
            if (privRealtor == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create(privRealtor);
                return RedirectToAction("Index");
            }
            return View("PrivRealtor", dataService.GetPrivRealtorView(privRealtor));
        }

        public IActionResult Edit(int? idRealtor)
        {
            ViewBag.Action = "Edit";
            if (idRealtor == null)
                return NotFound();
            var privRealtor = rc.PrivRealtors.FirstOrDefault(ort => ort.IdRealtor == idRealtor);
            if (privRealtor == null)
                return NotFound();
            return View("PrivRealtor", dataService.GetPrivRealtorView(privRealtor));
        }

        [HttpPost]
        public IActionResult Edit(PrivRealtor privRealtor)
        {
            if (privRealtor == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                dataService.Edit(privRealtor);
                return RedirectToAction("Index");
            }

            return View("PrivRealtor", dataService.GetPrivRealtorView(privRealtor));
        }

        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int? idRealtor)
        {
            ViewBag.Action = "Delete";
            if (idRealtor == null)
                return NotFound();
            var privRealtor = rc.PrivRealtors.FirstOrDefault(ort => ort.IdRealtor == idRealtor);
            if (privRealtor == null)
                return NotFound();
            return View("PrivRealtor", dataService.GetPrivRealtorView(privRealtor));
        }

        [HttpPost]
        public IActionResult Delete(PrivRealtor privRealtor)
        {
            if (privRealtor == null)
                return NotFound();

            var privRealtorOld = rc.PrivRealtors.FirstOrDefault(ort => ort.IdRealtor == privRealtor.IdRealtor);
            if (privRealtorOld != null)
            {
                dataService.Delete(privRealtorOld);
                return RedirectToAction("Index");
            }
            return Error("Не удалось удалить риелтора. Обратитесь к администратору");            
        }
    }
}