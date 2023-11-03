﻿using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryDb.Models;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.Filters;

namespace RegistryWeb.Controllers
{
    [Authorize]
    [HasPrivileges(Privileges.TenancyRead)]
    public class DocumentIssuedController: RegistryBaseController
    {
        private readonly RegistryContext rc;
        private readonly SecurityService securityService;
        private readonly DocumentIssuerService dataService;

        public DocumentIssuedController(RegistryContext rc, SecurityService securityService, DocumentIssuerService dataService)
        {
            this.rc = rc;
            this.securityService = securityService;
            this.dataService = dataService;
        }

        public IActionResult Index(PageOptions pageOptions)
        {
            return View(dataService.GetViewModel(pageOptions));
        }

        [HasPrivileges(Privileges.TenancyWrite)]
        public IActionResult Create()
        {
            ViewBag.Action = "Create";
            return View("DocumentIssue", dataService.GetDocumentIssuerView(new DocumentIssuedBy()));
        }

        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite)]
        public IActionResult Create(DocumentIssuedBy documentIssue)
        {
            if (documentIssue == null)
                return NotFound();
            if (ModelState.IsValid)
            {
                dataService.Create(documentIssue);
                return RedirectToAction("Index");
            }
            return View("DocumentIssue", dataService.GetDocumentIssuerView(documentIssue));
        }

        [HasPrivileges(Privileges.TenancyWrite)]
        public IActionResult Edit(int? idDocumentIssue)
        {
            ViewBag.Action = "Edit";
            if (idDocumentIssue == null)
                return NotFound();
            var documentIssue = rc.DocumentsIssuedBy.FirstOrDefault(ort => ort.IdDocumentIssuedBy == idDocumentIssue);
            if (documentIssue == null)
                return NotFound();
            return View("DocumentIssue", dataService.GetDocumentIssuerView(documentIssue));
        }

        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite)]
        public IActionResult Edit(DocumentIssuedBy documentIssue)
        {
            if (documentIssue == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                dataService.Edit(documentIssue);
                return RedirectToAction("Index");
            }

            return View("DocumentIssue", dataService.GetDocumentIssuerView(documentIssue));
        }

        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int? idDocumentIssue)
        {
            ViewBag.Action = "Delete";
            if (idDocumentIssue == null)
                return NotFound();
            var documentIssue = rc.DocumentsIssuedBy.FirstOrDefault(ort => ort.IdDocumentIssuedBy == idDocumentIssue);
            if (documentIssue == null)
                return NotFound();
            return View("DocumentIssue", dataService.GetDocumentIssuerView(documentIssue));
        }

        [HttpPost]
        [HasPrivileges(Privileges.TenancyWrite)]
        public IActionResult Delete(DocumentIssuedBy documentIssue)
        {
            if (documentIssue == null)
                return NotFound();

            var ownerReason = rc.TenancyPersons.FirstOrDefault(or => or.IdDocumentIssuedBy == documentIssue.IdDocumentIssuedBy);
            if (ownerReason == null)
            {
                var documentIssueold = rc.DocumentsIssuedBy.FirstOrDefault(ort => ort.IdDocumentIssuedBy == documentIssue.IdDocumentIssuedBy);
                if (documentIssueold != null)
                {
                    dataService.Delete(documentIssueold);
                    return RedirectToAction("Index");
                }
            }
            return Error("Нельзя удалить орган, выдающий документы, удостоверяющие личность, т.к. необходимо сначала удалить все зависимые записи!");            
        }
    }
}