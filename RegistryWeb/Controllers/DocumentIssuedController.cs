using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.Controllers
{
    [Authorize]
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

        public IActionResult Create()
        {
            ViewBag.Action = "Create";
            return View("DocumentIssue", dataService.GetDocumentIssuerView(new DocumentIssuedBy()));
        }

        [HttpPost]
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