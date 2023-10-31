using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryDb.Models;
using RegistryServices.ViewModel.RegistryObjects;

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
                KladrRegionsList = new SelectList(registryContext.KladrRegions, "IdRegion", "Region")
            };
            return viewModel;
        }
    }
}
