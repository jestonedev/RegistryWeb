using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.SecurityServices;

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

        public IViewComponentResult Invoke(int id, string action)
        {
            var model = new BuildingDemolitionInfoVM();
            model.BuildingDemolitionActFiles =
                registryContext.BuildingDemolitionActFiles
                .Include(b => b.ActTypeDocument)
                .Where(b => b.IdBuilding == id)
                .ToList();
            ViewBag.ActTypeDocuments =
                registryContext.ActTypeDocuments
                .Where(atd => atd.ActFileType == ActFileTypes.BuildingDemolitionActFile.ToString())
                .AsNoTracking();
            return View("BuildingDemolitionInfo", model);
        }
    }
}