﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Owners;
using RegistryServices.ViewModel.Owners;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class OwnerReasonTypesController : RegistryBaseController
    {
        private readonly RegistryContext rc;
        private readonly SecurityService securityService;

        public OwnerReasonTypesController(RegistryContext rc, SecurityService securityService)
        {
            this.rc = rc;
            this.securityService = securityService;
        }

        public IActionResult Index(PageOptions pageOptions)
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");

            var viewModel = new OwnerReasonTypesVM<OwnerReasonType>
            {
                PageOptions = pageOptions ?? new PageOptions()
            };
            var query = rc.OwnerReasonTypes.AsQueryable();
            viewModel.PageOptions.TotalRows = query.Count();
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.ownerReasonTypes = GetQueryPage(query, viewModel.PageOptions).ToList();

            ViewBag.Count = viewModel.ownerReasonTypes.Count();
            ViewBag.SecurityService = securityService;
            return View(viewModel);
        }

        public List<OwnerReasonType> GetQueryPage(IQueryable<OwnerReasonType> query, PageOptions pageOptions)
        {
            var result = query;
            result = result.Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage);
            result = result.Take(pageOptions.SizePage);
            return result.ToList();
        }

        public IActionResult Create()
        {
            ViewBag.Action = "Create";
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            return View();
        }

        public OwnerReasonTypesVM<OwnerReasonType> GetDocumentIssuerView(OwnerReasonType ownerReasonType)
        {
            var ownerReasonTypesVM = new OwnerReasonTypesVM<OwnerReasonType>()
            {
                ownerReasonType = ownerReasonType
            };
            return ownerReasonTypesVM;
        }

        [HttpPost]
        public IActionResult Create(OwnerReasonType ownerReasonType)
        {
            if (ownerReasonType == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                rc.OwnerReasonTypes.Add(ownerReasonType);
                rc.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(GetDocumentIssuerView(ownerReasonType));
        }

        public IActionResult Edit(int? idReasonType)
        {
            ViewBag.Action = "Edit";
            if (idReasonType == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
            if (ownerReasonTypes == null)
                return NotFound();
            return View(GetDocumentIssuerView(ownerReasonTypes));
        }

        [HttpPost]
        public IActionResult Edit(OwnerReasonType ownerReasonType)
        {
            if (ownerReasonType == null)
                return NotFound();

            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                rc.OwnerReasonTypes.Update(ownerReasonType);
                rc.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(GetDocumentIssuerView(ownerReasonType));
        }

        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int? idReasonType)
        {
            ViewBag.Action = "Delete";
            if (idReasonType == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
            if (ownerReasonTypes == null)
                return NotFound();
            return View(GetDocumentIssuerView(ownerReasonTypes));
        }

        [HttpPost]
        public IActionResult Delete(OwnerReasonType ownerReasonType)
        {
            if (ownerReasonType == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerDirectoriesReadWrite))
                return View("NotAccess");
            var ownerReason = rc.OwnerReasons.FirstOrDefault(or => or.IdReasonType == ownerReasonType.IdReasonType);
            if (ownerReason == null)
            {
                var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == ownerReasonType.IdReasonType);
                if (ownerReasonTypes != null)
                {
                    ownerReasonTypes.Deleted = 1;
                    rc.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return Error("Тип используется в основаниях собственности!");
        }
    }
}