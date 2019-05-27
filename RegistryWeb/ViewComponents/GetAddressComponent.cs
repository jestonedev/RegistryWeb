using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.ViewComponents
{
    public class GetAddressComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public GetAddressComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int idProcess)
        {
            var stringAddresses = new List<string>();

            stringAddresses.AddRange(GetBuildingAddresses(idProcess));
            stringAddresses.AddRange(GetPremisesAddresses(idProcess));
            stringAddresses.AddRange(GetSubPremisesAddresses(idProcess));

            return Content(string.Join("\n",stringAddresses));
        }

        private List<string> GetBuildingAddresses(int idProcess)
        {
            var addresses = new List<string>();
            var ownerBuildingsAssoc = registryContext.OwnerBuildingsAssoc
                .Include(oba => oba.IdBuildingNavigation)
                .Where(oba => oba.IdProcess == idProcess);
            foreach (var oba in ownerBuildingsAssoc)
            {
                addresses.Add(registryContext.KladrStreets
                    .Where(ks => ks.IdStreet == oba.IdBuildingNavigation.IdStreet)
                    .Select(ks => ks.StreetName).First() + ", д." + oba.IdBuildingNavigation.House);
            }
            return addresses;
        }

        private List<string> GetPremisesAddresses(int idProcess)
        {
            var addresses = new List<string>();
            var ownerPremisesAssoc = registryContext.OwnerPremisesAssoc
                .Include(opa => opa.IdPremisesNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                .Include(opa => opa.IdPremisesNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Where(opa => opa.IdProcess == idProcess);
            foreach (var opa in ownerPremisesAssoc)
            {
                addresses.Add(registryContext.KladrStreets
                    .Where(ks => ks.IdStreet == opa.IdPremisesNavigation.IdBuildingNavigation.IdStreet)
                    .Select(ks => ks.StreetName).First() + ", д." +
                    opa.IdPremisesNavigation.IdBuildingNavigation.House + ", " +
                    opa.IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                    opa.IdPremisesNavigation.PremisesNum);
            }
            return addresses;
        }

        private List<string> GetSubPremisesAddresses(int idProcess)
        {
            var addresses = new List<string>();
            var ownerSubPremisesAssoc = registryContext.OwnerSubPremisesAssoc
                .Include(ospa => ospa.IdSubPremisesNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                .Include(ospa => ospa.IdSubPremisesNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Where(ospa => ospa.IdProcess == idProcess);
            foreach (var ospa in ownerSubPremisesAssoc)
            {
                addresses.Add(registryContext.KladrStreets
                    .Where(ks => ks.IdStreet == ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet)
                    .Select(ks => ks.StreetName).First() + ", д." +
                    ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdBuildingNavigation.House + ", " +
                    ospa.IdSubPremisesNavigation.IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                    ospa.IdSubPremisesNavigation.IdPremisesNavigation.PremisesNum + ", к." +
                    ospa.IdSubPremisesNavigation.SubPremisesNum);
            }
            return addresses;
        }

    }
}
