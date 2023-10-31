using RegistryWeb.ViewOptions;
using System.Linq;

namespace RegistryServices.DataFilterServices
{
    public interface IFilterService<T, V>: IFilterServiceCommon
         where V : FilterOptions
         where T : class
    {
        IQueryable<T> GetQueryIncludes(IQueryable<T> query);
        IQueryable<T> GetQueryFilter(IQueryable<T> query, V filterOptions);
        IQueryable<T> GetQueryPage(IQueryable<T> query, PageOptions pageOptions);
        IQueryable<T> GetQueryOrder(IQueryable<T> query, OrderOptions orderOptions);
    }
}
