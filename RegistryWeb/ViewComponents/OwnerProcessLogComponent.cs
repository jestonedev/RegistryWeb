using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewComponents
{
    public class OwnerProcessLogComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnerProcessLogComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(IList<LogOwnerProcess> logOwnerProcess)
        {
            ViewBag.OwnerTypes = registryContext.OwnerType.AsNoTracking();
            ViewBag.ReasonTypes = registryContext.OwnerReasonTypes.AsNoTracking();
            ViewBag.Buildings = registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .AsNoTracking();
            ViewBag.Premises = registryContext.Premises
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdPremisesTypeNavigation)
                .AsNoTracking();
            ViewBag.SubPremises = registryContext.SubPremises
                .Include(sp => sp.IdPremisesNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(sp => sp.IdPremisesNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .AsNoTracking();
            return View("OwnerProcessLog", logOwnerProcess);
        }
    }
}
