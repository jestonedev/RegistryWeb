﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.Enums;

namespace RegistryWeb.ViewComponents
{
    public class ClaimsAddressComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public ClaimsAddressComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(Address address, int idClaim)
        {
            ViewData["IdClaim"] = idClaim;
            ViewData["Address"] = address;
            var id = int.Parse(address.Id);
            var model = new List<Address>();
            if (address.AddressType == AddressTypes.Premise)
            {
                var addr = new Address
                {
                    AddressType = AddressTypes.Building,
                    Id = address.IdParents[AddressTypes.Building.ToString()]
                };
                model.Add(addr);
                model.Add(address);
            }
            if (address.AddressType == AddressTypes.SubPremise)
            {
                var addr_b = new Address
                {
                    AddressType = AddressTypes.Building,
                    Id = address.IdParents[AddressTypes.Building.ToString()]
                };
                var addr_p = new Address
                {
                    AddressType = AddressTypes.Premise,
                    Id = address.IdParents[AddressTypes.Premise.ToString()]
                };
                model.Add(addr_b);
                model.Add(addr_p);
                model.Add(address);
            }
            return View("ClaimsAddress", model);
        }
    }
}
