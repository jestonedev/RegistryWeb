using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.Controllers
{
    public class ListController<LDS, F> : SessionController<F> where LDS : IListDataService where F : FilterOptions
    {
        protected readonly LDS dataService;
        protected readonly SecurityService securityService;

        public ListController(LDS dataService, SecurityService securityService)
        {
            this.dataService = dataService;
            this.securityService = securityService;
        }
    }
}