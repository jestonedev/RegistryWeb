using System;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.DataServices
{
    public class PrivatizationDataService : ListDataService<PrivatizationListVM, PrivatizationFilter>
    {
        public PrivatizationDataService(RegistryContext registryContext, AddressesDataService addressesDataService)
            : base(registryContext, addressesDataService)
        {
        }

        public override PrivatizationListVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PrivatizationFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }

        internal PrivatizationListVM GetViewModel(OrderOptions orderOptions, PageOptions pageOptions, PrivatizationFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var privContract = GetQuery();
            viewModel.PageOptions.TotalRows = privContract.Count();
            var query = privContract;
            //var query = GetQueryFilter(privContract, viewModel.FilterOptions);
            //query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.PrivContracts = GetQueryPage(query, viewModel.PageOptions).ToList();
            return viewModel;
        }

        private IQueryable<PrivContract> GetQuery()
        {
            return registryContext.PrivContracts.AsNoTracking();
        }

        private IQueryable<PrivContract> GetQueryPage(IQueryable<PrivContract> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        internal PrivContract GetPrivContract(int idContract)
        {
            var privContract = registryContext.PrivContracts
                .Include(pc => pc.ExecutorNavigation)
                .SingleOrDefault(pc => pc.IdContract == idContract);
            privContract.PrivContractors = registryContext.PrivContractors
                .Include(pc => pc.KinshipNavigation)
                .Where(pc => pc.IdContract == idContract)
                .ToList();
            return privContract;
        }

        internal List<Kinship> Kinships { get => registryContext.Kinships.ToList(); }

        public void Create(PrivContract contract)
        {
            registryContext.PrivContracts.Add(contract);
            registryContext.SaveChanges();
        }
    }
}