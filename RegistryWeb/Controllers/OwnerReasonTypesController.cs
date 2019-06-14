using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.Controllers
{
    public class OwnerReasonTypesController : Controller
    {
        private readonly RegistryContext rc;

        public OwnerReasonTypesController(RegistryContext rc)
        {
            this.rc = rc;
        }

        public IActionResult Index()
        {
            var ownerReasonTypes = rc.OwnerReasonTypes.ToList();
            return View(ownerReasonTypes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(OwnerReasonType ownerReasonTypes)
        {
            rc.OwnerReasonTypes.Add(ownerReasonTypes);
            rc.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int? idReasonType)
        {
            if (idReasonType != null)
            {
                var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
                if (ownerReasonTypes != null)
                    return View(ownerReasonTypes);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Edit(OwnerReasonType ownerReasonTypes)
        {
            rc.OwnerReasonTypes.Update(ownerReasonTypes);
            rc.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int? idReasonType)
        {
            if (idReasonType != null)
            {
                var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
                if (ownerReasonTypes != null)
                    return View(ownerReasonTypes);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Delete(int? idReasonType)
        {
            if (idReasonType != null)
            {
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
            return NotFound();
        }

        public IActionResult Error()
        {
            @ViewData["TextError"] = "Тип используется в основаниях собственности!";
            ViewData["Controller"] = "OwnerReasonTypes";
            return View("Error");
        }
    }
}