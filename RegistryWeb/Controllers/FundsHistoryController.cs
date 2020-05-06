using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    public class FundsHistoryController : RegistryBaseController
    {
        private readonly FundsHistoryDataService dataService;
        private readonly SecurityService securityService;

        public FundsHistoryController(FundsHistoryDataService dataService, SecurityService securityService)
        {
            this.dataService = dataService;
            this.securityService = securityService;
        }

        public IActionResult Index(int idObject, string typeObject, string action = "", bool isBack = false)
        {
            /*if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            if (!isBack)
            {
                viewModel.OrderOptions = HttpContext.Session.Get<OrderOptions>("OrderOptions");
                viewModel.PageOptions = HttpContext.Session.Get<PageOptions>("PageOptions");
                viewModel.FilterOptions = HttpContext.Session.Get<PremisesListFilter>("FilterOptions");
            }
            else
            {
                HttpContext.Session.Remove("OrderOptions");
                HttpContext.Session.Remove("PageOptions");
                HttpContext.Session.Remove("FilterOptions");
            }

            return View(dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions));*/
            ViewBag.Action = action;
            var viewModel = dataService.GetListViewModel(idObject, typeObject);
            ViewBag.addressType = typeObject;
            ViewBag.idObject = idObject;

            return View(/*"Index", */viewModel);//dataService.GetViewModel(viewModel));
        }

        public JsonResult Details(int idFund, string action)
        {
            ViewBag.Action = action;
            var fund = dataService.GetFundHistory(idFund);
            return Json(fund);
        }

        public IActionResult Create(int IdObject, string typeObject, string action = "")
        {
            ViewBag.Action = action;
            ViewBag.idObject = IdObject;
            ViewBag.addressType = typeObject;
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");

            return View("FundHistory", dataService.GetFundHistoryView(dataService.CreateFundHistory(), IdObject, typeObject));
        }

        [HttpPost]
        public IActionResult Create(FundHistoryVM fh, int IdObject, string typeObject, string action = "")
        {
            if (fh.FundHistory == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.OwnerWrite))
                return View("NotAccess");
            ViewBag.Action = "Index";
            if (ModelState.IsValid)
            {                
                dataService.Create(fh.FundHistory, IdObject, typeObject);
                //return RedirectToAction("Index");
                return View("Index", dataService.GetListViewModel(IdObject, typeObject));
            }
            return View("FundHistory", dataService.GetFundHistoryView(fh.FundHistory, IdObject, typeObject));
        }

        [HttpGet]
        public IActionResult Edit(int? idFund, int IdObject, string typeObject, string action = "")
        {
            ViewBag.Action = action;
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
        public IActionResult Edit(FundHistoryVM fh, int IdObject, string typeObject, string action = "")
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
            return View("FundHistory", dataService.GetFundHistoryView(dataService.CreateFundHistory(), IdObject, typeObject));
        }

        [HttpGet]
        public IActionResult Delete(int? idFund, int IdObject, string typeObject, string action = "")
        {
            ViewBag.Action = action;
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
        public IActionResult Delete(FundHistoryVM fh, int IdObject, string typeObject, string action = "")
        {
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
        }
    }
}