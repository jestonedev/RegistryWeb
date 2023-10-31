using RegistryDb.Models;
using System.Linq;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryServices.DataFilterServices
{
    class DocumentIssuedByFilterService : AbstractFilterService<DocumentIssuedBy, FilterOptions>
    {
        private readonly RegistryContext registryContext;

        public DocumentIssuedByFilterService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public override IQueryable<DocumentIssuedBy> GetQueryFilter(IQueryable<DocumentIssuedBy> query, FilterOptions filterOptions)
        {
            return query;
        }

        public override IQueryable<DocumentIssuedBy> GetQueryIncludes(IQueryable<DocumentIssuedBy> query)
        {
            return query;
        }

        public override IQueryable<DocumentIssuedBy> GetQueryOrder(IQueryable<DocumentIssuedBy> query, OrderOptions orderOptions)
        {
            return query;
        }
    }
}
