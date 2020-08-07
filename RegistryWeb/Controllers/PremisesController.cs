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
        private readonly RegistryContext rc;
        private readonly IConfiguration config;
        public PremisesController(RegistryContext rc, PremisesDataService dataService, SecurityService securityService, IConfiguration config)
            : base(dataService, securityService)
        {
            this.rc = rc;
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

        public IActionResult SessionIdPremiseRemove(int idPremises)
        {
            var ids = HttpContext.Session.Get<List<int>>("idPremises");
            ids.Remove(idPremises);
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
                    var viewModel = new PremisesVM<Premise> {
                        Premises = dataService.GetPremises(ids) ?? new List<Premise>(),
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
                    return View("PremiseReports", viewModel);
                }
            }
            ViewBag.Count = 0;
            return View("PremiseReports", new PremisesVM<Premise>());
        }


//_________________Для проставления____________________ 
        [HttpPost]
        public IActionResult AddRestrictionInPremises(Restriction restriction, Address address, IFormFile restrictionFile, bool restrictionFileRemove)
        {
            if (restriction == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(new { Error = -2 });

            List<int> ids;
            if (HttpContext.Session.Keys.Contains("idPremises"))
                ids = HttpContext.Session.Get<List<int>>("idPremises");
            else ids = new List<int>();

            if (ids == null)
                return NotFound();

            var premise = dataService.GetPremises(ids);

            for (var i=0;i<ids.Count();i++)
            {
                var rest = new Restriction
                {
                    Number=restriction.Number,
                    Date=restriction.Date,
                    DateStateReg=restriction.DateStateReg,
                    Description=restriction.Description,
                    IdRestrictionType=restriction.IdRestrictionType,
                    FileDisplayName=restriction.FileDisplayName,
                    FileMimeType=restriction.FileMimeType,
                    FileOriginName=restriction.FileOriginName
                };

                var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Restrictions\");

                if (restrictionFile != null && !restrictionFileRemove)
                {
                    rest.FileDisplayName = restrictionFile.FileName;
                    rest.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(restrictionFile.FileName).Extension;
                    rest.FileMimeType = restrictionFile.ContentType;
                    var fileStream = new FileStream(Path.Combine(path, rest.FileOriginName), FileMode.CreateNew);
                    restrictionFile.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }

                //Создать
                if (rest.IdRestriction == 0)
                {
                    rc.Restrictions.Add(rest);
                    //rc.SaveChanges();
                    var rpa = new RestrictionPremiseAssoc()
                    {
                        IdPremises = premise[i].IdPremises,
                        IdRestriction = rest.IdRestriction
                    };
                    rc.RestrictionPremisesAssoc.Add(rpa);
                    //rc.SaveChanges();
                }
            }
            //rc.SaveChanges();
            return Json(0);
            //return PremiseReports();
            //return Json(new { restriction.IdRestriction, restriction.FileOriginName });
        }        

        [HttpPost]
        public IActionResult AddOwnershipInPremises(OwnershipRight ownershipRight, Address address, IFormFile ownershipRightFile, bool ownershipRightFileRemove)
        {
            if (ownershipRight == null)
                return Json(new { Error = -1 });
            if (!securityService.HasPrivilege(Privileges.RegistryReadWriteNotMunicipal) &&
                !securityService.HasPrivilege(Privileges.RegistryReadWriteMunicipal))
                return Json(new { Error = -2 });

            List<int> ids;
            if (HttpContext.Session.Keys.Contains("idPremises"))
                ids = HttpContext.Session.Get<List<int>>("idPremises");
            else ids = new List<int>();

            if (ids == null)
                return NotFound();

            var premise = dataService.GetPremises(ids);

            for (var i = 0; i < ids.Count(); i++)
            {
                var owr = new OwnershipRight
                {
                    Number = ownershipRight.Number,
                    Date = ownershipRight.Date,
                    Description = ownershipRight.Description,
                    IdOwnershipRightType = ownershipRight.IdOwnershipRightType,
                    FileDisplayName = ownershipRight.FileDisplayName,
                    FileMimeType = ownershipRight.FileMimeType,
                    FileOriginName = ownershipRight.FileOriginName
                };

                var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Restrictions\");

                if (ownershipRightFile != null && !ownershipRightFileRemove)
                {
                    owr.FileDisplayName = ownershipRightFile.FileName;
                    owr.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(ownershipRightFile.FileName).Extension;
                    owr.FileMimeType = ownershipRightFile.ContentType;
                    var fileStream = new FileStream(Path.Combine(path, owr.FileOriginName), FileMode.CreateNew);
                    ownershipRightFile.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }

                //Создать
                if (owr.IdOwnershipRight == 0)
                {
                    rc.OwnershipRights.Add(owr);
                    //rc.SaveChanges();
                    var opa = new OwnershipPremiseAssoc()
                    {
                        IdPremises = premise[i].IdPremises,
                        IdOwnershipRight = owr.IdOwnershipRight
                    };
                    rc.OwnershipPremisesAssoc.Add(opa);
                    //rc.SaveChanges();
                }
            }
            //rc.SaveChanges();
            return Json(0);
            //return PremiseReports();
            //return Json(new { ownershipRight.IdOwnershipRight, ownershipRight.FileOriginName });
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

            if (ModelState.IsValid)
            {
                dataService.UpdateInfomationInPremises(premise, description, regDate, stateId);
                //return RedirectToAction("PremiseReports", new { premise.IdPremises });
            }            

            ViewBag.SecurityService = securityService;
            //return Json(0);
            return PremiseReports();
        }

    }
}
