using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models;
using RegistryWeb.SecurityServices;
using System;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.RegistryObjects;

namespace RegistryWeb.ViewComponents
{
    public class BuildingDemolitionInfoComponent : ViewComponent
    {
        private RegistryContext registryContext;
        private SecurityService securityService;

        public BuildingDemolitionInfoComponent(RegistryContext registryContext, SecurityService securityService)
        {
            this.registryContext = registryContext;
            this.securityService = securityService;
        }

        public IViewComponentResult Invoke(int id, DateTime? demolishedPlanDate, DateTime? demolishedFactDate, 
            DateTime? dateOwnerEmergency, DateTime? demandForDemolishingDeliveryDate, ActionTypeEnum action)
        {
            var model = new BuildingDemolitionInfoVM();
            model.BuildingDemolitionActFiles =
                registryContext.BuildingDemolitionActFiles
                .Include(b => b.ActTypeDocument)
                .Include(b => b.ActFile)
                .Where(b => b.IdBuilding == id)
                .OrderBy(b => b.Id)
                .ToList();
            model.DemolishedPlanDate = demolishedPlanDate;
            model.DemolishedFactDate = demolishedFactDate;
            model.DateOwnerEmergency = dateOwnerEmergency;
            model.DemandForDemolishingDeliveryDate = demandForDemolishingDeliveryDate;
            model.IdBuilding = id;
            ViewBag.ActTypeDocuments =
                registryContext.ActTypeDocuments
                .Where(atd => atd.ActFileType == ActFileTypes.BuildingDemolitionActFile.ToString())
                .AsNoTracking();
            ViewBag.Action = action;
            ViewBag.CanEditExtInfo = securityService.HasPrivilege(Privileges.RegistryWriteDemolishingInfo);
            return View("BuildingDemolitionInfo", model);
        }
    }
}