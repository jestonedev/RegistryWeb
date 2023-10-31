using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RegistryWeb.ViewOptions;

namespace RegistryServices.DataFilterServices
{
    abstract class AbstractFilterService<T, V> : IFilterService<T, V>
         where V : FilterOptions
         where T : class
    {
        public abstract IQueryable<T> GetQueryFilter(IQueryable<T> query, V filterOptions);

        public abstract IQueryable<T> GetQueryIncludes(IQueryable<T> query);
        public abstract IQueryable<T> GetQueryOrder(IQueryable<T> query, OrderOptions orderOptions);

        public IQueryable<T> GetQueryPage(IQueryable<T> query, PageOptions pageOptions)
        {
            return query
            .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
            .Take(pageOptions.SizePage);
        }
    }
}
