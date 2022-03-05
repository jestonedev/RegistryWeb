using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;
using RegistryWeb.Enums;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryWeb.ViewComponents
{
    public class TenancyProcessesComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public TenancyProcessesComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(Address address, string returnUrl)
        {
            IEnumerable<TenancyProcess> model = null;
            int id = 0;
            if (int.TryParse(address.Id, out id))
            {
                if (address.AddressType == AddressTypes.SubPremise)
                {
                    model = registryContext.TenancySubPremisesAssoc
                        .Where(tspa => tspa.IdSubPremise == id)
                        .Include(tspa => tspa.ProcessNavigation)
                        .Select(tspa => tspa.ProcessNavigation)
                        .Include(tp => tp.IdRentTypeNavigation)
                        .Include(tp => tp.TenancyPersons);
                }
                if (address.AddressType == AddressTypes.Premise)
                {
                    model = registryContext.TenancyPremisesAssoc
                        .Where(tpa => tpa.IdPremise == id)
                        .Include(tpa => tpa.ProcessNavigation)
                        .Select(tpa => tpa.ProcessNavigation)
                        .Include(tp => tp.IdRentTypeNavigation)
                        .Include(tp => tp.TenancyPersons);
                }
            }
            ViewBag.RentTypes = registryContext.RentTypes;
            ViewBag.ReturnUrl = returnUrl;
            return View("TenancyProcessesTable", model);
        }
    }
}
