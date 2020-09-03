using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.DataServices
{
    public class RegistryObjectsDataService
    {
        private readonly RegistryContext registryContext;
        public RegistryObjectsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public RegistryObjectReportsVM GetViewModel()
        {
            var viewModel = new RegistryObjectReportsVM
            {
                KladrRegionsList = new SelectList(registryContext.KladrRegions, "id_region", "region")
            };
            return viewModel;
        }
    }
}
