using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Extensions;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Runtime.CompilerServices;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using RegistryWeb.Models;
using RegistryWeb.DataHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PremisesController : ListController<PremisesDataService, PremisesListFilter>
    {
        private readonly IConfiguration config;
        public PremisesController(PremisesDataService dataService, SecurityService securityService, IConfiguration config)
            : base(dataService, securityService)
        {
            this.config = config;

            nameFilteredIdsDict = "filteredPremisesIdsDict";
            nameIds = "idPremises";
            nameMultimaster = "PremiseReports";
        }

        public IActionResult Index(PremisesVM<Premise> viewModel, string action="", bool isBack = false)
        {
            if (viewModel.PageOptions != null && viewModel.PageOptions.CurrentPage < 1)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");

            if (isBack)
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
            ViewBag.SecurityService = securityService;

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions, out List<int> filteredPremisesIds);

            AddSearchIdsToSession(vm.FilterOptions, filteredPremisesIds);

            return View(vm);
        }

        public IActionResult Details(int? idPremises, string returnUrl)
        {
            ViewBag.Action = ActionTypeEnum.Details;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            if (idPremises == null)
                return NotFound();
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();
            ViewBag.CanEditBaseInfo = CanEditPremiseBaseInfo(premise);
            ViewBag.CanDeleteBaseInfo = CanDeleteBaseInfo(premise);
            ViewBag.CanEditAnySubPremises = CanEditAnySubPremises(premise);
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: false));
        }

        public IActionResult Create(int? idBuilding)
        {
            ViewBag.Action = ActionTypeEnum.Create;
            ViewBag.SecurityService = securityService;
            if (!securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal))
                return View("NotAccess");
            ViewBag.CanEditBaseInfo = true;
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);
            return View("Premise", dataService.GetPremiseView(dataService.CreatePremise(idBuilding), canEditBaseInfo: true));
        }

        [HttpPost]
        public IActionResult Create(Premise premise, int? IdFundType, List<int?> subPremisesFundTypes)
        {
            if (premise == null)
                return NotFound();

            if (!CanEditPremiseBaseInfo(premise) || (premise.SubPremises.Count > 0 && !CanEditAnySubPremises(premise)))
                return View("NotAccess");
            if (!securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo))
            {
                premise.ResettlePremisesAssoc = null;
            }
            if (!securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo))
            {
                premise.LitigationPremisesAssoc = null;
            }
            if (ModelState.IsValid)
            {
                if (premise.SubPremises != null)
                {
                    for(var i = 0; i < premise.SubPremises.Count; i++)
                    {
                        if (subPremisesFundTypes[i] != null)
                        {
                            var subPremise = premise.SubPremises[i];

                            var fund = new FundHistory
                            {
                                IdFundType = subPremisesFundTypes[i].Value
                            };

                            var fspa = new FundSubPremiseAssoc
                            {
                                IdFundNavigation = fund,
                                IdSubPremisesNavigation = subPremise
                            };
                            
                            subPremise.FundsSubPremisesAssoc = new List<FundSubPremiseAssoc> { fspa };
                        }
                    }
                }

                dataService.Create(premise, HttpContext.Request.Form.Files.Select(f => f).ToList(), IdFundType);
                return RedirectToAction("Details", new { premise.IdPremises });
            }
            ViewBag.Action = ActionTypeEnum.Create;
            ViewBag.SecurityService = securityService;
            ViewBag.CanEditBaseInfo = true;
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: true));
        }

        [HttpGet]
        public IActionResult Edit(int? idPremises, string returnUrl)
        {
            ViewBag.Action = ActionTypeEnum.Edit;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            if (idPremises == null)
                return NotFound();

            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();

            ViewBag.CanEditBaseInfo = CanEditPremiseBaseInfo(premise);
            ViewBag.CanDeleteBaseInfo = CanDeleteBaseInfo(premise);
            ViewBag.CanEditAnySubPremises = CanEditAnySubPremises(premise);
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);

            if (!(bool)ViewBag.CanEditBaseInfo && !(bool)ViewBag.CanEditResettleInfo && !(bool)ViewBag.CanEditLitigationInfo)
                return View("NotAccess");
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: (bool)ViewBag.CanEditBaseInfo));
        }

        [HttpPost]
        public IActionResult Edit(Premise premise, string returnUrl)
        {
            if (premise == null)
                return NotFound();

            ViewBag.CanEditBaseInfo = CanEditPremiseBaseInfo(premise);
            ViewBag.CanDeleteBaseInfo = CanDeleteBaseInfo(premise);
            ViewBag.CanEditAnySubPremises = CanEditAnySubPremises(premise);
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);

            if (!(bool)ViewBag.CanEditBaseInfo)
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                dataService.Edit(premise);
                string returnUrlPrepared = null;
                if (returnUrl?.Split("&returnUrl=", 2).Length == 2)
                    returnUrlPrepared = System.Net.WebUtility.UrlDecode(returnUrl.Split("&returnUrl=")[1]);
                return RedirectToAction("Details", new { premise.IdPremises, returnUrl = returnUrlPrepared });
            }
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.Action = ActionTypeEnum.Edit;
            ViewBag.SecurityService = securityService;
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: (bool)ViewBag.CanEditBaseInfo));
        }

        [HttpGet]
        public IActionResult Delete(int? idPremises, string returnUrl)
        {
            ViewBag.Action = ActionTypeEnum.Delete;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            if (idPremises == null)
                return NotFound();

            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();

            if (!CanEditPremiseBaseInfo(premise) || (premise.SubPremises.Count > 0 && !CanEditAnySubPremises(premise)))
                return View("NotAccess");

            ViewBag.CanEditBaseInfo = false;
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);

            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: false));
        }

        [HttpPost]
        public IActionResult Delete(Premise premise, string returnUrl)
        {
            if (premise == null)
                return NotFound();

            var premiseDb = dataService.GetPremise(premise.IdPremises);

            if (!CanEditPremiseBaseInfo(premiseDb) || (premiseDb.SubPremises.Count > 0 && !CanEditAnySubPremises(premiseDb)))
                return View("NotAccess");

            ViewBag.ReturnUrl = returnUrl;
            dataService.Delete(premise.IdPremises);
            return RedirectToAction("Index");
        }

        private bool CanEditPremiseBaseInfo(Premise premise)
        {
            return (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.IsMunicipal(premise.IdState)) ||
                   (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.IsMunicipal(premise.IdState));
        }

        private bool CanEditAnySubPremises(Premise premise)
        {
            return (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && 
                        premise.SubPremises.Any(sp => ObjectStateHelper.IsMunicipal(sp.IdState))) ||
                   (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) &&
                        premise.SubPremises.Any(sp => !ObjectStateHelper.IsMunicipal(sp.IdState)));
        }

        private bool CanDeleteBaseInfo(Premise premise)
        {
            return  (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal)) ||
                    (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.IsMunicipal(premise.IdState) && !premise.SubPremises.Any(sp => !ObjectStateHelper.IsMunicipal(sp.IdState))) ||
                    (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.IsMunicipal(premise.IdState) && !premise.SubPremises.Any(sp => ObjectStateHelper.IsMunicipal(sp.IdState)));
        }

        [HttpPost]
        public void SessionIdPremises(int idPremise, bool isCheck)
        {
            List<int> ids = GetSessionPremisesIds();

            if (isCheck)
                ids.Add(idPremise);
            else if (ids.Any())
                ids.Remove(idPremise);

            HttpContext.Session.Set("idPremises", ids);
        }

        public IActionResult PremiseReports(PageOptions pageOptions)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            var canEditBaseInfo =   
                securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) ||
                securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal);
            ViewBag.CanEditBaseInfo = canEditBaseInfo;
            ViewBag.CanReadTenancyInfo = securityService.HasPrivilege(Privileges.TenancyRead);
            var errorIds = new List<int>();
            if (TempData.ContainsKey("ErrorPremisesIds"))
            {
                try
                {
                    errorIds = JsonConvert.DeserializeObject<List<int>>(TempData["ErrorPremisesIds"].ToString());
                }
                catch
                {
                }
                TempData.Remove("ErrorPremisesIds");
            }

            ViewBag.ErrorPremises = dataService.GetPremisesForMassReports(errorIds).ToList();
            var ids = GetSessionPremisesIds();
            var viewModel = dataService.GetPremisesViewModelForMassReports(ids, pageOptions, canEditBaseInfo);
            ViewBag.Count = viewModel.Premises.Count();
            return View("PremiseReports", viewModel);
        }

        private List<int> GetSessionPremisesIds()
        {
            if (HttpContext.Session.Keys.Contains("idPremises"))
                return HttpContext.Session.Get<List<int>>("idPremises");
            return new List<int>();
        }
        
        [HttpPost]
        public IActionResult AddRestrictionInPremises(Restriction restriction, IFormFile restrictionFile)
        {
            if (restriction == null)
                return Error("Не передана ссылка на реквизит права муниципальной собственности");
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Error("У вас нет прав на выполнение данной операции");

            List<int> ids = GetSessionPremisesIds();

            if (!ids.Any())
                return NotFound();

            var premises = dataService.GetPremisesForMassReports(ids).ToList();
            var processingPremises = new List<Premise>();
            var errorPremisesIds = new List<int>();
            foreach (Premise premise in premises) 
            {
                var canEditBaseInfo = CanEditPremiseBaseInfo(premise);
                if (canEditBaseInfo)
                {
                    processingPremises.Add(premise);
                } else
                {
                    errorPremisesIds.Add(premise.IdPremises); // Уведомление
                }
            }

            dataService.AddRestrictionsInPremises(processingPremises, restriction, restrictionFile);
            TempData["ErrorPremisesIds"] = JsonConvert.SerializeObject(errorPremisesIds);
            return RedirectToAction("PremiseReports");
        }

        [HttpPost]
        public IActionResult AddOwnershipInPremises(OwnershipRight ownershipRight, IFormFile ownershipRightFile)
        {
            if (ownershipRight == null)
                return Error("Не передана ссылка на ограничение");
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Error("У вас нет прав на выполнение данной операции");

            List<int> ids = GetSessionPremisesIds();

            if (!ids.Any())
                return NotFound();

            var premises = dataService.GetPremisesForMassReports(ids).ToList();
            var processingPremises = new List<Premise>();
            var errorPremisesIds = new List<int>();
            foreach (Premise premise in premises)
            {
                var canEditBaseInfo = CanEditPremiseBaseInfo(premise);
                if (canEditBaseInfo)
                {
                    processingPremises.Add(premise);
                }
                else
                {
                    errorPremisesIds.Add(premise.IdPremises); // Уведомление
                }
            }

            dataService.AddOwnershipRightInPremises(processingPremises, ownershipRight, ownershipRightFile);
            TempData["ErrorPremisesIds"] = JsonConvert.SerializeObject(errorPremisesIds);
            return RedirectToAction("PremiseReports");
        }

        [HttpPost]
        public IActionResult UpdateInfoInPremises(string description, DateTime? regDate, int? stateId)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                 !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(new { Error = -2 });

            List<int> ids = GetSessionPremisesIds();

            if (!ids.Any())
                return NotFound();

            var premises = dataService.GetPremisesForMassReports(ids).ToList();
            var processingPremises = new List<Premise>();
            var errorPremisesIds = new List<int>();
            foreach (Premise premise in premises)
            {
                var canEditBaseInfo = CanEditPremiseBaseInfo(premise);
                if (canEditBaseInfo)
                {
                    processingPremises.Add(premise);
                }
                else
                {
                    errorPremisesIds.Add(premise.IdPremises);
                }
            }

            dataService.UpdateInfomationInPremises(processingPremises, description, regDate, stateId);
            TempData["ErrorPremisesIds"] = JsonConvert.SerializeObject(errorPremisesIds);
            return RedirectToAction("PremiseReports");
        }

        [HttpPost]
        public IActionResult GetAreaAvgCostView()
        {
            ViewBag.SecurityService = securityService;
            return View("AreaAvgCost", dataService.GetAreaAvgCost());
        }

        [HttpPost]
        public IActionResult UpdateAreaAvgCost(TotalAreaAvgCost avgCost)
        {
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
            {
                return Json(-1);
            }
            if (ModelState.IsValid)
            {
                dataService.UpdateAreaAvgCost(avgCost);
            }
            return Json(1);
        }
    }
}
