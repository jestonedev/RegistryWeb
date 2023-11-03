using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using RegistryDb.Models.Entities.Owners;
using RegistryServices.ViewModel.Owners;
using RegistryWeb.Filters;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.OwnerRead)]
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

        [HasPrivileges(Privileges.OwnerDirectoriesReadWrite)]
        public IActionResult Create()
        {
            ViewBag.Action = "Create";
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
        [HasPrivileges(Privileges.OwnerDirectoriesReadWrite)]
        public IActionResult Create(OwnerReasonType ownerReasonType)
        {
            if (ownerReasonType == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                rc.OwnerReasonTypes.Add(ownerReasonType);
                rc.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(GetDocumentIssuerView(ownerReasonType));
        }

        [HasPrivileges(Privileges.OwnerDirectoriesReadWrite)]
        public IActionResult Edit(int? idReasonType)
        {
            ViewBag.Action = "Edit";
            if (idReasonType == null)
                return NotFound();
            var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
            if (ownerReasonTypes == null)
                return NotFound();
            return View(GetDocumentIssuerView(ownerReasonTypes));
        }

        [HttpPost]
        [HasPrivileges(Privileges.OwnerDirectoriesReadWrite)]
        public IActionResult Edit(OwnerReasonType ownerReasonType)
        {
            if (ownerReasonType == null)
                return NotFound();

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
        [HasPrivileges(Privileges.OwnerDirectoriesReadWrite)]
        public IActionResult ConfirmDelete(int? idReasonType)
        {
            ViewBag.Action = "Delete";
            if (idReasonType == null)
                return NotFound();
            var ownerReasonTypes = rc.OwnerReasonTypes.FirstOrDefault(ort => ort.IdReasonType == idReasonType);
            if (ownerReasonTypes == null)
                return NotFound();
            return View(GetDocumentIssuerView(ownerReasonTypes));
        }

        [HttpPost]
        [HasPrivileges(Privileges.OwnerDirectoriesReadWrite)]
        public IActionResult Delete(OwnerReasonType ownerReasonType)
        {
            if (ownerReasonType == null)
                return NotFound();
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