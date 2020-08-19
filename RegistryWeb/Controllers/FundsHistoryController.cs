using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class FundsHistoryController : RegistryBaseController
    {
        private readonly FundsHistoryDataService dataService;
        private readonly SecurityService securityService;
        private readonly RegistryContext rc;

        public FundsHistoryController(RegistryContext rc, FundsHistoryDataService dataService, SecurityService securityService)
        {
            this.rc = rc;
            this.dataService = dataService;
            this.securityService = securityService;
        }

        public IActionResult Index(int idObject, string typeObject, string returnUrl, bool isBack = false)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
                        
            ViewBag.Action = "Index";
            ViewBag.ReturnUrl = returnUrl;
            var viewModel = dataService.GetListViewModel(idObject, typeObject);
            ViewBag.addressType = typeObject;
            ViewBag.idObject = idObject;

            if(typeObject=="SubPremise")
            {
                ViewBag.Num = rc.SubPremises.FirstOrDefault(s=>s.IdSubPremises==idObject).SubPremisesNum;
                ViewBag.Prem = rc.SubPremises.FirstOrDefault(s=>s.IdSubPremises==idObject).IdPremises;
            }

            return View(viewModel);
        }

        public JsonResult Details(int idFund, string returnUrl)
        {
            ViewBag.Action = "Details";
            ViewBag.ReturnUrl = returnUrl;
            var fund = dataService.GetFundHistory(idFund);
            return Json(fund);
        }

        public IActionResult Create(int IdObject, string typeObject, string returnUrl)
        {
            ViewBag.Action = "Create";
            ViewBag.idObject = IdObject;
            ViewBag.addressType = typeObject;
            ViewBag.ReturnUrl = returnUrl;
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");

            return View("FundHistory", dataService.GetFundHistoryView(dataService.CreateFundHistory(), IdObject, typeObject));
        }

        [HttpPost]
        public IActionResult Create(FundHistoryVM fh, int IdObject, string typeObject, string returnUrl)
        {
            if (fh.FundHistory == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            ViewBag.Action = "Create";
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {                
                dataService.Create(fh.FundHistory, IdObject, typeObject);
                return View("Index", dataService.GetListViewModel(IdObject, typeObject));
            }
            return View("FundHistory", dataService.GetFundHistoryView(fh.FundHistory, IdObject, typeObject));
        }

        [HttpGet]
        public IActionResult Edit(int? idFund, int IdObject, string typeObject, string returnUrl)
        {
            ViewBag.Action = "Edit";
            ViewBag.ReturnUrl = returnUrl;
            if (idFund == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");

            var fh = dataService.GetFund(idFund.Value);
            if (fh == null)
                return NotFound();

            return View("FundHistory", dataService.GetFundHistoryView(fh, IdObject, typeObject));
        }

        [HttpPost]
        public IActionResult Edit(FundHistoryVM fh, int IdObject, string typeObject, string returnUrl)
        {
            if (fh.FundHistory == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Edit(fh.FundHistory);
                return View("Index", dataService.GetListViewModel(IdObject, typeObject));
                //return RedirectToAction("Index");
            }
            ViewBag.Action = "Edit";
            ViewBag.ReturnUrl = returnUrl;
            return View("FundHistory", dataService.GetFundHistoryView(dataService.CreateFundHistory(), IdObject, typeObject));
        }

        [HttpGet]
        public IActionResult Delete(int? idFund, int IdObject, string typeObject, string returnUrl)
        {
            ViewBag.Action = "Delete";
            ViewBag.ReturnUrl = returnUrl;
            if (idFund == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            var fh = dataService.GetFund(idFund.Value);
            if (fh == null)
                return NotFound();

            return View("FundHistory", dataService.GetFundHistoryView(fh, IdObject, typeObject));
        }

        [HttpPost]
        public IActionResult Delete(FundHistoryVM fh, int IdObject, string typeObject, string returnUrl)
        {
            ViewBag.Action = "Delete";
            ViewBag.ReturnUrl = returnUrl;
            if (fh.FundHistory == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            if (ModelState.IsValid)
            {
                dataService.Delete(fh.FundHistory.IdFund);
                return View("Index", dataService.GetListViewModel(IdObject, typeObject));
                //return RedirectToAction("Index");
            }
            return View("FundHistory", dataService.GetFundHistoryView(fh.FundHistory, 0, ""));
        }/**/
    }
}