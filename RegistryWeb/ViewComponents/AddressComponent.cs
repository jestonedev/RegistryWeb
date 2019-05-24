using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;

namespace RegistryWeb.ViewComponents
{
    public class AddressComponent : ViewComponent
    {
        private RegistryContext registryContext;

        public AddressComponent(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public IViewComponentResult Invoke(int id)
        {
            ViewBag.Id = id;
            return View("Address", registryContext.KladrStreets);
        }
    }
}
