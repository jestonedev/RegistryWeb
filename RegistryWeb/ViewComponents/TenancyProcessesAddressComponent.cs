using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.ViewComponents
{
    public class TenancyProcessesAddressComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public TenancyProcessesAddressComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(Address address, int idProcess)
        {
            ViewData["IdProcess"] = idProcess;
            ViewData["Address"] = address;
            var id = int.Parse(address.Id);
            var model = new List<Address>();
            if (address.AddressType == AddressTypes.Building)
            {
                model.Add(address);
            }
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
            return View("TenancyProcessesAddress", model);
        }
    }
}
