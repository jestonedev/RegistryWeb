using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    public class OwnerReasonsController : Controller
    {
        private readonly RegistryContext rc;

        public OwnerReasonsController(RegistryContext rc)
        {
            this.rc = rc;
        }

        public IActionResult Index()
        {
            var ownerReasons = rc.OwnerReasons
                .Include(or => or.IdReasonTypeNavigation)
                .OrderBy(or => or.IdReason)
                .ToList();
            return View(ownerReasons);
        }

        public IActionResult Create()
        {
            var viewModel = new OwnerReasonEditVM();
            viewModel.OwnerReason = new OwnerReasons();
            viewModel.OwnerReasonTypes = rc.OwnerReasonTypes.ToList();
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(OwnerReasons ownerReason)
        {
            var existOwnerReason = rc.OwnerReasons
                .FirstOrDefault(or => or.IdReasonType == ownerReason.IdReasonType &&
                                or.ReasonDate == ownerReason.ReasonDate &&
                                or.ReasonNumber == ownerReason.ReasonNumber);
            if (existOwnerReason == null)
            {
                rc.OwnerReasons.Add(ownerReason);
                rc.SaveChanges();
                return RedirectToAction("Index");
            }
            return Error();
        }

        public IActionResult Edit(int? idReason)
        {
            if (idReason != null)
            {
                var ownerReasons = rc.OwnerReasons.FirstOrDefault(ort => ort.IdReason == idReason);
                if (ownerReasons != null)
                {
                    var viewModel = new OwnerReasonEditVM();
                    viewModel.OwnerReason = ownerReasons;
                    viewModel.OwnerReasonTypes = rc.OwnerReasonTypes.ToList();
                    return View(viewModel);
                }
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Edit(OwnerReasonEditVM viewModel)
        {
            rc.OwnerReasons.Update(viewModel.OwnerReason);
            rc.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int? idReason)
        {
            if (idReason != null)
            {
                var ownerReasons = rc.OwnerReasons
                    .Include(or => or.IdReasonTypeNavigation)
                    .FirstOrDefault(or => or.IdReason == idReason);
                if (ownerReasons != null)
                    return View(ownerReasons);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult Delete(int? idReason)
        {
            if (idReason != null)
            {
                var ownerReason = rc.OwnerReasons.FirstOrDefault(or => or.IdReason == idReason);
                if (ownerReason != null)
                {
                    ownerReason.Deleted = 1;
                    rc.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return NotFound();
        }

        public IActionResult Error([CallerMemberName]string name = "")
        {
            if (name == "Create")
                ViewData["TextError"] = "Не удалось созадать новое основание найма!";
            
            ViewData["Controller"] = "OwnerReasons";
            return View("Error");
        }
    }
}