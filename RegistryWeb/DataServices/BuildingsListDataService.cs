using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;
using System;

namespace RegistryWeb.DataServices
{
    public class BuildingsListDataService : ListDataService<BuildingsListVM, BuildingsListFilter>
    {
        public BuildingsListDataService(RegistryContext registryContext) : base(registryContext) { }

        public override BuildingsListVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, BuildingsListFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.ObjectStates = registryContext.ObjectStates;
            return viewModel;
        }

        public BuildingsListVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            BuildingsListFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.Buildings = GetQueryPage(query, viewModel.PageOptions);
            return viewModel;
        }

        public IQueryable<Buildings> GetQuery()
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)
                .Include(b => b.IdStructureTypeNavigation)
                .Where(b => b.Deleted == 0)
                .OrderBy(b => b.IdBuilding);
        }
        private IQueryable<Buildings> GetQueryFilter(IQueryable<Buildings> query, BuildingsListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.Street))
            {
                query = query.Where(b => b.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(filterOptions.Street.ToLowerInvariant()));
            }
            if (filterOptions.IdObjectState.HasValue)
            {
                query = query.Where(b =>b.IdStateNavigation.IdState == filterOptions.IdObjectState.Value);
            }
            return query;
        }

        private IQueryable<Buildings> GetQueryOrder(IQueryable<Buildings> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdBuilding")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(b => b.IdBuilding);
                else
                    return query.OrderByDescending(b => b.IdBuilding);
            }
            if (orderOptions.OrderField == "ObjectState")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(b => b.IdStateNavigation.StateNeutral);
                else
                    return query.OrderByDescending(b => b.IdStateNavigation.StateNeutral);
            }
            return query;
        }

        public List<Buildings> GetQueryPage(IQueryable<Buildings> query, PageOptions pageOptions)
            => query
            .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
            .Take(pageOptions.SizePage).ToList();
    }
}
