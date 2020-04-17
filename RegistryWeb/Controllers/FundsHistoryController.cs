using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    public class FundsHistoryController : RegistryBaseController
    {
        private readonly FundsHistoryDataService dataService;

        public FundsHistoryController(FundsHistoryDataService dataService)
        {
            this.dataService = dataService;
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

            var viewModel = dataService.GetListViewModel(idObject, typeObject);
            ViewBag.addressType = typeObject;
            ViewBag.idObject = idObject;

            return View(/*"Index", */viewModel);//dataService.GetViewModel(viewModel));
        }

        /*public IActionResult GetFund(int idFund)
        {
            var fund = dataService.GetFundHistory(idFund);
            return View("FundHistory", fund);//dataService.GetViewModel(viewModel)), dataService.GetFundViewModel(fund));
        }*/

        public JsonResult GetFund(int idFund)
        {
            var fund = dataService.GetFundHistory(idFund);
            return Json(fund);
        }
    }
}