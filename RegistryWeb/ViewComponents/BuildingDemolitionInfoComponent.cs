using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.SecurityServices;
using System;

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

        public IViewComponentResult Invoke(int id, DateTime? demolishPlanDate, string action)
        {
            var model = new BuildingDemolitionInfoVM();
            model.BuildingDemolitionActFiles =
                registryContext.BuildingDemolitionActFiles
                .Include(b => b.ActTypeDocument)
                .Where(b => b.IdBuilding == id)
                .ToList();
            model.DemolishPlanDate = demolishPlanDate;
            ViewBag.ActTypeDocuments =
                registryContext.ActTypeDocuments
                .Where(atd => atd.ActFileType == ActFileTypes.BuildingDemolitionActFile.ToString())
                .AsNoTracking();
            ViewBag.IdBuilding = id;
            ViewBag.Action = action;
            return View("BuildingDemolitionInfo", model);
        }
    }
}