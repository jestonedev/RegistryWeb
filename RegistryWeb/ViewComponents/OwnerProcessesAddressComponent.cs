using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;
using RegistryWeb.Enums;

namespace RegistryWeb.ViewComponents
{
    public class OwnerProcessesAddressComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public OwnerProcessesAddressComponent(RegistryContext registryContext)
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
                var addr = new Address();
                addr.AddressType = AddressTypes.Building;
                addr.Id = registryContext.Premises
                    .SingleOrDefault(p => p.IdPremises == id)
                    .IdBuilding
                    .ToString();
                model.Add(addr);
                model.Add(address);
            }
            if (address.AddressType == AddressTypes.SubPremise)
            {
                var premise = registryContext.SubPremises
                    .Include(sp => sp.IdPremisesNavigation)
                    .SingleOrDefault(sp => sp.IdSubPremises == id)
                    .IdPremisesNavigation;
                var addr_b = new Address();
                addr_b.AddressType = AddressTypes.Building;
                addr_b.Id = premise.IdBuilding.ToString();
                var addr_p = new Address();
                addr_p.AddressType = AddressTypes.Premise;
                addr_p.Id = premise.IdPremises.ToString();
                model.Add(addr_b);
                model.Add(addr_p);
                model.Add(address);
            }
            return View("OwnerProcessesAddress", model);
        }
    }
}
