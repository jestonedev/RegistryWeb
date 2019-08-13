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
    public class ChangeLogsDataService : ListDataService<ChangeLogsVM, ChangeLogsFilter>
    {
        public ChangeLogsDataService(RegistryContext registryContext) : base(registryContext) { }

        public override ChangeLogsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, ChangeLogsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }

        public ChangeLogsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            ChangeLogsFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.ChangeLogs = GetQueryPage(query, viewModel.PageOptions);
            return viewModel;
        }

        public IQueryable<ChangeLog> GetQuery()
        {
            return registryContext.ChangeLogs;
        }

        private IQueryable<ChangeLog> GetQueryFilter(IQueryable<ChangeLog> query, ChangeLogsFilter filterOptions)
        {
            return query;
        }

        private IQueryable<ChangeLog> GetQueryOrder(IQueryable<ChangeLog> query, OrderOptions orderOptions)
        {
            return query;
        }

        public List<ChangeLog> GetQueryPage(IQueryable<ChangeLog> query, PageOptions pageOptions)
            => query
            .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
            .Take(pageOptions.SizePage).ToList();
    }
}
