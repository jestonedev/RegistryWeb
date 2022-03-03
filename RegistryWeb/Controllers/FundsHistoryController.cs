using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.DataHelpers;
using RegistryDb.Models;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryServices.ViewModel.RegistryObjects;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class FundsHistoryController : RegistryBaseController
    {
        private readonly FundsHistoryDataService dataService;
        private readonly BuildingsDataService buildingService;
        private readonly SecurityService securityService;
        private readonly RegistryContext rc;

        public FundsHistoryController(RegistryContext rc, FundsHistoryDataService dataService, SecurityService securityService, BuildingsDataService buildingService)
        {
            this.rc = rc;
            this.dataService = dataService;
            this.securityService = securityService;
            this.buildingService = buildingService;
        }

        public IActionResult Index(int idObject, string typeObject, string returnUrl, bool isBack = false)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
                        
            ViewBag.Action = "Index";
            ViewBag.ReturnUrl = returnUrl;
            var viewModel = dataService.GetListViewModel(idObject, typeObject);
            ViewBag.typeObject = typeObject;
            ViewBag.idObject = idObject;

            if(typeObject=="SubPremise")
            {
                ViewBag.Num = rc.SubPremises.FirstOrDefault(s=>s.IdSubPremises==idObject).SubPremisesNum;
                ViewBag.Prem = rc.SubPremises.FirstOrDefault(s=>s.IdSubPremises==idObject).IdPremises;
            }

            return View(viewModel);
        }

        public IActionResult Details(int idFund)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteMunicipal))    //если будет необх., то RegistryWriteMunicipal надо убрать
                return View("NotAccess");

            var fund = dataService.GetFundHistory(idFund);
            return Json(fund);
        }

        public IActionResult Create(int IdObject, string typeObject, string returnUrl)
        {
            ViewBag.Action = "Create";
            ViewBag.idObject = IdObject;
            ViewBag.typeObject = typeObject;
            ViewBag.ReturnUrl = returnUrl;
            if (!securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteMunicipal))    //если будет необх., то RegistryWriteMunicipal надо убрать
                return View("NotAccess");

            if (!CanEditFundHistory(IdObject, typeObject))
                return View("NotAccess");

            if (typeObject == "SubPremise")
            {
                ViewBag.Num = rc.SubPremises.FirstOrDefault(s => s.IdSubPremises == IdObject).SubPremisesNum;
                ViewBag.Prem = rc.SubPremises.FirstOrDefault(s => s.IdSubPremises == IdObject).IdPremises;
            }

            return View("FundHistory", dataService.GetFundHistoryView(dataService.CreateFundHistory(), IdObject, typeObject));
        }

        [HttpPost]
        public IActionResult Create(FundHistoryVM fh, int IdObject, string typeObject, string returnUrl)
        {
            if (fh.FundHistory == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteMunicipal))    //если будет необх., то RegistryWriteMunicipal надо убрать
                return View("NotAccess");

            if (!CanEditFundHistory(IdObject, typeObject))
                return View("NotAccess");

            ViewBag.Action = "Index";
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {                
                dataService.Create(fh.FundHistory, IdObject, typeObject);
                return Redirect(returnUrl);
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
            if (!securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteMunicipal))    //если будет необх., то RegistryWriteMunicipal надо убрать
                return View("NotAccess");

            if (!CanEditFundHistory(IdObject, typeObject))
                return View("NotAccess");

            var fh = dataService.GetFund(idFund.Value);
            if (fh == null)
                return NotFound();

            if (typeObject == "SubPremise")
            {
                ViewBag.Num = rc.SubPremises.FirstOrDefault(s => s.IdSubPremises == IdObject).SubPremisesNum;
                ViewBag.Prem = rc.SubPremises.FirstOrDefault(s => s.IdSubPremises == IdObject).IdPremises;
            }

            return View("FundHistory", dataService.GetFundHistoryView(fh, IdObject, typeObject));
        }

        [HttpPost]
        public IActionResult Edit(FundHistoryVM fh, int IdObject, string typeObject, string returnUrl)
        {
            if (fh.FundHistory == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteMunicipal))    //если будет необх., то RegistryWriteMunicipal надо убрать
                return View("NotAccess");

            if (!CanEditFundHistory(IdObject, typeObject))
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                dataService.Edit(fh.FundHistory, IdObject, typeObject);
                return Redirect(returnUrl);
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
            if (!securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteMunicipal))    //если будет необх., то RegistryWriteMunicipal надо убрать
                return View("NotAccess");

            if (!CanEditFundHistory(IdObject, typeObject))
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
            if (!securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteMunicipal))    //если будет необх., то RegistryWriteMunicipal надо убрать
                return View("NotAccess");

            if (!CanEditFundHistory(IdObject, typeObject))
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                dataService.Delete(fh.FundHistory.IdFund);
                return Redirect(returnUrl);
            }
            return View("FundHistory", dataService.GetFundHistoryView(fh.FundHistory, 0, ""));
        }/**/

        public bool CanEditFundHistory(int IdObject, string typeObject)
        {
            if (typeObject == "Building")
            {
                var building = rc.Buildings
                    .FirstOrDefault(op => op.IdBuilding == IdObject);
                if ((securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !buildingService.IsMunicipal(building)) ||
                    (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && buildingService.IsMunicipal(building)))
                    return true;
            }
            else if (typeObject == "Premise")
            {
                var premise = rc.Premises
                    .FirstOrDefault(op => op.IdPremises == IdObject);
                if ((securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.IsMunicipal(premise.IdState)) ||
                    (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.IsMunicipal(premise.IdState)))
                    return true;
            }
            else if (typeObject == "SubPremise")
            {
                var subPremise = rc.SubPremises
                    .FirstOrDefault(op => op.IdSubPremises == IdObject);
                if ((securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.IsMunicipal(subPremise.IdState)) ||
                    (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.IsMunicipal(subPremise.IdState)))
                    return true;
            }
            else return false;

            return false;
        }
    }
}