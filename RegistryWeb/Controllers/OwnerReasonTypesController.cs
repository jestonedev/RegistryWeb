using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerReasonTypesController : Controller
    {
        private readonly RegistryContext rc;
        private readonly SecurityService securityService;

        public OwnerReasonTypesController(RegistryContext rc, SecurityService securityService)
        {
            this.rc = rc;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            var ownerReasonTypes = rc.OwnerReasonTypes.ToList();
            ViewBag.SecurityService = securityService;
            return View(ownerReasonTypes);
        }

        public IActionResult Create()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            return View();
        }

        [HttpPost]
        public IActionResult Create(OwnerReasonType ownerReasonTypes)
        {
            if (ownerReasonTypes == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            rc.OwnerReasonTypes.Add(ownerReasonTypes);
            rc.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? idReasonType)
        {
            if (idReasonType == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
            if (ownerReasonTypes == null)
                return NotFound();
            return View(ownerReasonTypes);
        }

        [HttpPost]
        public IActionResult Edit(OwnerReasonType ownerReasonTypes)
        {
            if (ownerReasonTypes == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            rc.OwnerReasonTypes.Update(ownerReasonTypes);
            rc.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int? idReasonType)
        {
            if (idReasonType == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
            if (ownerReasonTypes == null)
                return NotFound();
            return View(ownerReasonTypes);
        }

        [HttpPost]
        public IActionResult Delete(int? idReasonType)
        {
            if (idReasonType == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            var ownerReason = rc.OwnerReasons.FirstOrDefault(or => or.IdReasonType == idReasonType);
            if (ownerReason == null)
            {
                var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
                if (ownerReasonTypes != null)
                {
                    ownerReasonTypes.Deleted = 1;
                    rc.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Error");
        }

        public IActionResult Error()
        {
            @ViewData["TextError"] = "Тип используется в основаниях собственности!";
            ViewData["Controller"] = "OwnerReasonTypes";
            return View("Error");
        }
    }
}