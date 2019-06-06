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

        public IViewComponentResult Invoke(IAddressAssoc addressAssoc, int id, string action)
        {
            ViewBag.Id = id;
            ViewBag.Action = action;
            ViewBag.ClassAssoc = "buildingBlock";
            ViewBag.KladrStreets = registryContext.KladrStreets.AsNoTracking();
            ViewBag.Buildings = new List<Buildings>();
            ViewBag.PremisesTypes = new List<PremisesTypes>();
            ViewBag.Premises = new List<Premises>();
            ViewBag.SubPremises = new List<SubPremises>();

            var address = GetAddress(addressAssoc);

            if (address.IdTypeAddress != 0)
            { 
                ViewBag.Buildings = registryContext.Buildings
                    .Where(b => b.IdStreet == address.IdStreet)
                    .AsNoTracking();
                ViewBag.ClassAssoc = "buildingBlock";
                if (address.IdTypeAddress == 2 || address.IdTypeAddress == 3)
                {
                    ViewBag.PremisesTypes = registryContext.Premises
                        .Include(p => p.IdPremisesTypeNavigation)
                        .Where(p => p.IdBuilding == address.IdBuilding)
                        .Select(p => p.IdPremisesTypeNavigation)
                        .Distinct()
                        .AsNoTracking();
                    ViewBag.Premises = registryContext.Premises
                        .Where(p => p.IdBuilding == address.IdBuilding && p.IdPremisesType == address.IdPremisesType)
                        .AsNoTracking();
                    ViewBag.ClassAssoc = "premiseBlock";
                    if (address.IdTypeAddress == 3)
                    {
                        ViewBag.SubPremises = registryContext.SubPremises
                            .Where(sp => sp.IdPremises == address.IdPremise)
                            .AsNoTracking();
                        ViewBag.ClassAssoc = "subPremiseBlock";
                    }
                }
            }
            return View("Address", address);
        }

        private Address GetAddress(IAddressAssoc addressAssoc)
        {
            var address = new Address();
            if (addressAssoc is OwnerBuildingsAssoc)
            {
                var oba = (OwnerBuildingsAssoc)addressAssoc;
                address.IdProcess = oba.IdProcess;
                address.IdAssoc = oba.IdAssoc;
                address.IdTypeAddress = 1;
                var buildings = registryContext.Buildings
                    .Where(b => b.IdBuilding == oba.IdBuilding)
                    .AsNoTracking();
                if (buildings.Count() == 0)
                    return address;
                var building = buildings.First();
                address.IdStreet = building.IdStreet;
                address.IdBuilding = building.IdBuilding;
            }
            if (addressAssoc is OwnerPremisesAssoc)
            {
                var opa = (OwnerPremisesAssoc)addressAssoc;
                address.IdProcess = opa.IdProcess;
                address.IdAssoc = opa.IdAssoc;
                address.IdTypeAddress = 2;
                var premise = registryContext.Premises
                    .Include(p => p.IdBuildingNavigation)
                    .AsNoTracking()
                    .First(p => p.IdPremises == opa.IdPremises);
                address.IdStreet = premise.IdBuildingNavigation.IdStreet;
                address.IdBuilding = premise.IdBuilding;
                address.IdPremisesType = premise.IdPremisesType;
                address.IdPremise = premise.IdPremises;
            }
            if (addressAssoc is OwnerSubPremisesAssoc)
            {
                var ospa = (OwnerSubPremisesAssoc)addressAssoc;
                address.IdProcess = ospa.IdProcess;
                address.IdAssoc = ospa.IdAssoc;
                address.IdTypeAddress = 3;
                var subPremise = registryContext.SubPremises
                    .Include(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                    .AsNoTracking()
                    .First(sp => sp.IdSubPremises == ospa.IdSubPremises);
                address.IdStreet = subPremise.IdPremisesNavigation.IdBuildingNavigation.IdStreet;
                address.IdBuilding = subPremise.IdPremisesNavigation.IdBuilding;
                address.IdPremisesType = subPremise.IdPremisesNavigation.IdPremisesType;
                address.IdPremise = subPremise.IdPremisesNavigation.IdPremises;
                address.IdSubPremise = subPremise.IdSubPremises;
            }
            return address;
        }
    }
}
