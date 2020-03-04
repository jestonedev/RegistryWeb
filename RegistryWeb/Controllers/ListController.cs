using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    public class ListController<LDS> : RegistryBaseController where LDS : IListDataService
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