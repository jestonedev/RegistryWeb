using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryDb.Models;
using RegistryWeb.ViewModel;

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
