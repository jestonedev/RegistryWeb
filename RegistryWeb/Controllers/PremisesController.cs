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
    public class PremisesController : ListController<PremisesDataService>
    {
        private readonly IConfiguration config;
        public PremisesController(PremisesDataService dataService, SecurityService securityService, IConfiguration config)
            : base(dataService, securityService)
        {
            this.config = config;
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
            //ViewBag.PremiseService = dataService;

            var vm = dataService.GetViewModel(
                viewModel.OrderOptions,
                viewModel.PageOptions,
                viewModel.FilterOptions, out List<int> filteredPremisesIds);

            AddSearchPremisesIdsToSession(vm.FilterOptions, filteredPremisesIds);

            return View(vm);
        }

        public IActionResult Details(int? idPremises, string returnUrl)
        {
            ViewBag.Action = "Details";
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
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: false));
        }

        public IActionResult Create()
        {
            ViewBag.Action = "Create";
            ViewBag.SecurityService = securityService;
            if (!securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal))
                return View("NotAccess");
            ViewBag.CanEditBaseInfo = true;
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);
            return View("Premise", dataService.GetPremiseView(dataService.CreatePremise(), canEditBaseInfo: true));
        }

        [HttpPost]
        public IActionResult Create(Premise premise, int? IdFundType, List<int?> subPremisesFundTypes)
        {
            if (premise == null)
                return NotFound();

            if (!CanEditPremiseBaseInfo(premise))
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
            ViewBag.Action = "Create";
            ViewBag.SecurityService = securityService;
            ViewBag.CanEditBaseInfo = true;
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: true));
        }

        [HttpGet]
        public IActionResult Edit(int? idPremises, string returnUrl)
        {
            ViewBag.Action = "Edit";
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            if (idPremises == null)
                return NotFound();

            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();

            ViewBag.CanEditBaseInfo = CanEditPremiseBaseInfo(premise);
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
            ViewBag.CanEditResettleInfo = securityService.HasPrivilege(Privileges.RegistryWriteResettleInfo);
            ViewBag.CanEditLitigationInfo = securityService.HasPrivilege(Privileges.RegistryWriteLitigationInfo);

            if (!(bool)ViewBag.CanEditBaseInfo)
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                dataService.Edit(premise);
                return RedirectToAction("Details", new { premise.IdPremises });
            }
            ViewBag.Action = "Edit";
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: (bool)ViewBag.CanEditBaseInfo));
        }

        [HttpGet]
        public IActionResult Delete(int? idPremises, string returnUrl)
        {
            ViewBag.Action = "Delete";
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SecurityService = securityService;
            if (idPremises == null)
                return NotFound();

            var premise = dataService.GetPremise(idPremises.Value);
            if (premise == null)
                return NotFound();

            if (!CanEditPremiseBaseInfo(premise))
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

            if (!CanEditPremiseBaseInfo(premiseDb))
                return View("NotAccess");

            ViewBag.ReturnUrl = returnUrl;
            dataService.Delete(premise.IdPremises);
            return RedirectToAction("Index");
        }

        private bool CanEditPremiseBaseInfo(Premise premise)
        {
            return ((securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.IsMunicipal(premise.IdState) &&
                (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) || premise.SubPremises == null || !premise.SubPremises.Any(sp => ObjectStateHelper.IsMunicipal(sp.IdState)))) ||
               (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.IsMunicipal(premise.IdState)));
        }

        public JsonResult GetHouse(string streetId)
        {
            IEnumerable<Building> buildings = dataService.GetHouses(streetId);
            return Json(buildings);
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

        public void AddSearchPremisesIdsToSession(PremisesListFilter filterOptions, List<int> filteredPremisesIds)
        {
            var filteredPremisesIdsDict = new Dictionary<string, List<int>>();

            if (HttpContext.Session.Keys.Contains("filteredPremisesIdsDict"))
                filteredPremisesIdsDict = HttpContext.Session.Get<Dictionary<string, List<int>>>("filteredPremisesIdsDict");

            var filterOptionsSerialized = JsonConvert.SerializeObject(filterOptions).ToString();

            if (filteredPremisesIdsDict.Keys.Contains(filterOptionsSerialized))
                filteredPremisesIdsDict[filterOptionsSerialized] = filteredPremisesIds;
            else filteredPremisesIdsDict.Add(filterOptionsSerialized, filteredPremisesIds);

            HttpContext.Session.Set("filteredPremisesIdsDict", filteredPremisesIdsDict);
        }

        public IActionResult AddSessionSelectedAndFilteredPremises(PremisesListFilter filterOptions)
        {
            if (!HttpContext.Session.Keys.Contains("filteredPremisesIdsDict"))
                return Json(0);

            var filteredPremisesIdsDict = HttpContext.Session.Get<Dictionary<string, List<int>>>("filteredPremisesIdsDict");
            var filterOptionsSerialized = JsonConvert.SerializeObject(filterOptions).ToString();

            if (filteredPremisesIdsDict.Keys.Contains(filterOptionsSerialized))
            {
                List<int> filterOptionsIds = filteredPremisesIdsDict[filterOptionsSerialized];
                List<int> ids = GetSessionPremisesIds();
                ids.AddRange(filterOptionsIds);
                ids = ids.Distinct().ToList();
                HttpContext.Session.Set("idPremises", ids);
                return Json(0);
            }
            return Json(-1);
        }

        public IActionResult SessionIdPremisesClear()
        {
            HttpContext.Session.Remove("idPremises");
            ViewBag.Count = 0;
            return RedirectToAction("PremiseReports");
        }

        public IActionResult SessionIdPremiseRemove(int idPremises)
        {
            var ids = GetSessionPremisesIds();
            ids.Remove(idPremises);
            ViewBag.Count = ids.Count();
            HttpContext.Session.Set("idPremises", ids);
            return RedirectToAction("PremiseReports");
        }

        public IActionResult PremiseReports()
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            var canEditBaseInfo =   
                securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) ||
                securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal);
            ViewBag.CanEditBaseInfo = canEditBaseInfo;
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
            if (errorIds.Any())
            {
                ViewBag.ErrorPremises = dataService.GetPremises(errorIds);
            }
            var viewModel = dataService.InitializeViewModel(null, null, null);
            viewModel.ObjectStatesList = new SelectList(dataService.GetObjectStatesWithRights("Edit", canEditBaseInfo), "IdState", "StateFemale");
            viewModel.CommisionList = viewModel.SignersList;
            var ids = GetSessionPremisesIds();
            if (ids.Any())
            {
                viewModel.Premises = dataService.GetPremises(ids);
            } else
            {
                viewModel.Premises = new List<Premise>();
            }
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

            var premises = dataService.GetPremises(ids);
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

            var premises = dataService.GetPremises(ids);
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

            var premises = dataService.GetPremises(ids);
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

    }
}
