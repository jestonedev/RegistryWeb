using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;
using RegistryWeb.Enums;

namespace RegistryWeb.ViewComponents
{
    public class TenancyProcessesAddressComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public TenancyProcessesAddressComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(TenancyRentObject rentObject, int idProcess)
        {
            ViewData["IdProcess"] = idProcess;
            ViewData["Address"] = rentObject.Address;
            ViewData["TotalArea"] = rentObject.TotalArea;
            ViewData["LivingArea"] = rentObject.LivingArea;
            var id = int.Parse(rentObject.Address.Id);
            var model = new List<Address>();
            if (rentObject.Address.AddressType == AddressTypes.Building)
            {
                model.Add(rentObject.Address);
            }
            if (rentObject.Address.AddressType == AddressTypes.Premise)
            {
                var addr = new Address
                {
                    AddressType = AddressTypes.Building,
                    Id = rentObject.Address.IdParents[AddressTypes.Building.ToString()]
                };
                model.Add(addr);
                model.Add(rentObject.Address);
            }
            if (rentObject.Address.AddressType == AddressTypes.SubPremise)
            {
                var addr_b = new Address
                {
                    AddressType = AddressTypes.Building,
                    Id = rentObject.Address.IdParents[AddressTypes.Building.ToString()]
                };
                var addr_p = new Address
                {
                    AddressType = AddressTypes.Premise,
                    Id = rentObject.Address.IdParents[AddressTypes.Premise.ToString()]
                };
                model.Add(addr_b);
                model.Add(addr_p);
                model.Add(rentObject.Address);
            }
            return View("TenancyProcessesAddress", model);
        }
    }
}
