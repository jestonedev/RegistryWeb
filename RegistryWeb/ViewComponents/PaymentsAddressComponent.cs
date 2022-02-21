using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;
using RegistryWeb.Enums;

namespace RegistryWeb.ViewComponents
{
    public class PaymentsAddressComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public PaymentsAddressComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(Address address, int idAccount)
        {
            ViewData["IdAccount"] = idAccount;
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
            return View("PaymentsAddress", model);
        }
    }
}
