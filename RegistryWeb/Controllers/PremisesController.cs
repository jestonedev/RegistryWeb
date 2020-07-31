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

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class PremisesController : ListController<PremisesDataService>
    {
        private readonly RegistryContext rc;
        public PremisesController(RegistryContext rc, PremisesDataService dataService, SecurityService securityService)
            : base(dataService, securityService)
        {
            this.rc = rc;
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
            ViewBag.PremiseService = dataService;

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
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: false));
        }

        public IActionResult Create()
        {
            ViewBag.Action = "Create";
            ViewBag.SecurityService = securityService;
            if (!securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && !securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal))
                return View("NotAccess");
            ViewBag.CanEditBaseInfo = true;
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);
            return View("Premise", dataService.GetPremiseView(dataService.CreatePremise(), canEditBaseInfo: true));
        }

        [HttpPost]
        public IActionResult Create(Premise premise, int? IdFundType, List<int?> subPremisesFundTypes)
        {
            if (premise == null)
                return NotFound();

            if (!CanEditPremiseBaseInfo(premise))
                return View("NotAccess");
            if (!securityService.HasPrivilege(Privileges.RegistryWriteExtInfo))
            {
                premise.ResettlePremisesAssoc = null;
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
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);
            return View("Create", dataService.GetPremiseView(premise, canEditBaseInfo: true));
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
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);

            if (!(bool)ViewBag.CanEditBaseInfo && !(bool)ViewBag.CanEditExtInfo)
                return View("NotAccess");
            return View("Premise", dataService.GetPremiseView(premise, canEditBaseInfo: (bool)ViewBag.CanEditBaseInfo));
        }

        [HttpPost]
        public IActionResult Edit(Premise premise, string returnUrl)
        {
            if (premise == null)
                return NotFound();

            ViewBag.CanEditBaseInfo = CanEditPremiseBaseInfo(premise);
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);

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
            return View("Edit", dataService.GetPremiseView(premise, canEditBaseInfo: (bool)ViewBag.CanEditBaseInfo));
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
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);

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
            List<int> ids;
            if (HttpContext.Session.Keys.Contains("idPremises"))            
                ids = HttpContext.Session.Get<List<int>>("idPremises");            
            else ids = new List<int>();
            
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
                List<int> ids = new List<int>();
                if (HttpContext.Session.Keys.Contains("idPremises"))
                    ids = HttpContext.Session.Get<List<int>>("idPremises");
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
            return PremiseReports();
        }

        public IActionResult SessionIdPremiseRemove(int idBuilding)
        {
            var ids = HttpContext.Session.Get<List<int>>("idPremises");
            ids.Remove(idBuilding);
            ViewBag.Count = ids.Count();
            HttpContext.Session.Set("idPremises", ids);
            return PremiseReports();
        }

        public IActionResult PremiseReports()
        {
            if (!securityService.HasPrivilege(Privileges.RegistryRead))
                return View("NotAccess");
            if (HttpContext.Session.Keys.Contains("idPremises"))
            {
                var ids = HttpContext.Session.Get<List<int>>("idPremises");
                if (ids.Any())
                {
                    ViewBag.Count = ids.Count();
                    
                    //var premises = dataService.GetPremises(ids);
                    var viewModel = new PremisesVM<Premise> {
                        Premises = dataService.GetPremises(ids),
                        SignersList = new SelectList(rc.SelectableSigners.Where(s => s.IdSignerGroup == 1).ToList().Select(s => new {
                            s.IdRecord,
                            Snp = s.Surname + " " + s.Name + (s.Patronymic == null ? "" : " " + s.Patronymic)
                        }), "IdRecord", "Snp"),
                        CommisionList = new SelectList(rc.SelectableSigners.Where(s => s.IdSignerGroup == 1).ToList().Select(s => new {
                            s.IdRecord,
                            Snp = s.Surname + " " + s.Name + (s.Patronymic == null ? "" : " " + s.Patronymic)
                        }), "IdRecord", "Snp"),
                        PreparersList = new SelectList(rc.Preparers, "IdPreparer", "PreparerName"),
                        ObjectStatesList= new SelectList(rc.ObjectStates, "IdState", "StateFemale"),
                        OwnershipRightTypesList = new SelectList(rc.OwnershipRightTypes, "IdOwnershipRightType", "OwnershipRightTypeName"),
                        RestrictionsList = new SelectList(rc.RestrictionTypes, "IdRestrictionType", "RestrictionTypeName")
                    };

                    //return View("PremiseReports", premises);
                    return View("PremiseReports", viewModel);
                }
            }
            return View("PremiseReports", new List<Premise>());
        }


//_________________Для проставления____________________ 
        [HttpPost]
        public IActionResult AddRestrictionInPremises(Restriction restriction)
        //public IActionResult AddRestrictionInPremises(string number, DateTime daterest, int resttype, string description, DateTime dateStateReg)
        {
            if(restriction==null)
                return Json(-1);
            else
            {

            List<int> ids;
            if (HttpContext.Session.Keys.Contains("idPremises"))            
                ids = HttpContext.Session.Get<List<int>>("idPremises");            
            else ids = new List<int>();            

            if (ids == null)
                return NotFound();

            var premise = dataService.GetPremises(ids);
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);

            if (!(bool)ViewBag.CanEditBaseInfo)
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                //var restriction = new Restriction { Number=number, Date=daterest, IdRestrictionType=resttype, Description=description, DateStateReg=dateStateReg };
                dataService.UpdateRestrictionInPremises(restriction, premise);
                //return RedirectToAction("PremiseReports", new { premise.IdPremises });
            }            
            
            ViewBag.SecurityService = securityService;
            //return View("Edit", dataService.GetPremiseView(premise, canEditBaseInfo: (bool)ViewBag.CanEditBaseInfo));
            return PremiseReports();
            }
        }

        [HttpPost]
        public IActionResult AddOwnershipInPremises(OwnershipRight ownership)
        {
            List<int> ids;
            if (HttpContext.Session.Keys.Contains("idPremises"))            
                ids = HttpContext.Session.Get<List<int>>("idPremises");            
            else ids = new List<int>();            

            if (ids == null)
                return NotFound();

            var premise = dataService.GetPremises(ids);
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);

            if (!(bool)ViewBag.CanEditBaseInfo)
                return View("NotAccess");

            if (ModelState.IsValid)
            {
                dataService.UpdateOwnershipRightInPremises(ownership, premise);
                //return RedirectToAction("PremiseReports", new { premise.IdPremises });
            }

            ViewBag.SecurityService = securityService;
            //return View("Edit", dataService.GetPremiseView(premise, canEditBaseInfo: (bool)ViewBag.CanEditBaseInfo));
            return PremiseReports();
        }

        [HttpPost]
        public IActionResult UpdatePremises(string description, DateTime regDate, int stateId)
        {
            List<int> ids;
            if (HttpContext.Session.Keys.Contains("idPremises"))            
                ids = HttpContext.Session.Get<List<int>>("idPremises");            
            else ids = new List<int>();            

            if (ids == null)
                return NotFound();

            var premise = dataService.GetPremises(ids);
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteExtInfo);

            /*if (!(bool)ViewBag.CanEditBaseInfo)
                return View("NotAccess");*/

            if (ModelState.IsValid)
            {
                dataService.UpdateInfomationInPremises(premise, description, regDate, stateId);
                //return RedirectToAction("PremiseReports", new { premise.IdPremises });
            }            

            ViewBag.SecurityService = securityService;
            //return View("Edit", dataService.GetPremiseView(premise, canEditBaseInfo: (bool)ViewBag.CanEditBaseInfo));
            return PremiseReports();
        }




    }
}
