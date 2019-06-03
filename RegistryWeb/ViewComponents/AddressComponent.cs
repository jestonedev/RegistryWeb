using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.ViewComponents
{
    public class AddressComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public AddressComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(Address address, int id, string action)
        {
            ViewBag.Id = id;
            ViewBag.Action = action;
            ViewBag.KladrStreets = registryContext.KladrStreets;
            ViewBag.Buildings = new List<Buildings>();
            ViewBag.PremisesTypes = new List<PremisesTypes>();
            ViewBag.Premises = new List<Premises>();
            ViewBag.SubPremises = new List<SubPremises>();

            if (address.IdTypeAddress != 0)
            { 
                ViewBag.Buildings = registryContext.Buildings
                    .Where(b => b.IdStreet == address.IdStreet);
                if (address.IdTypeAddress == 2 || address.IdTypeAddress == 3)
                {
                    ViewBag.PremisesTypes = registryContext.Premises
                        .Include(p => p.IdPremisesTypeNavigation)
                        .Where(p => p.IdBuilding == address.IdBuilding)
                        .Select(p => p.IdPremisesTypeNavigation)
                        .Distinct();
                    ViewBag.Premises = registryContext.Premises
                        .Where(p => p.IdBuilding == address.IdBuilding && p.IdPremisesType == address.IdPremisesType);
                    if (address.IdTypeAddress == 3)
                    {
                        ViewBag.SubPremises = registryContext.SubPremises
                            .Where(sp => sp.IdSubPremises == address.IdSubPremise);
                    }
                }
            }
            return View("Address", address);
        }
    }
}
