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

        internal Address GetAddressRegistry(PrivContract contract)
        {
            if (contract.IdPremise != null)
            {
                var premise = registryContext.Premises
                    .Include(p => p.IdPremisesTypeNavigation)
                    .Include(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                    .SingleOrDefault(p => p.IdPremises == contract.IdPremise);
                return new Address
                {
                    Id = premise?.IdPremises.ToString(),
                    AddressType = AddressTypes.Premise,
                    Text = premise?.GetAddress(),
                    IdParents = new Dictionary<string, string>
                    {
                        {"IdStreet", premise?.IdBuildingNavigation?.IdStreet },
                        {"IdBuilding", premise?.IdBuilding.ToString() },
                        {"IdPremise", premise?.IdPremises.ToString() },
                        {"IdSubPremise", null }
                    }
                };
            }
            if (contract.IdSubPremise != null)
            {
                var subPremise = registryContext.SubPremises
                    .Include(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                    .Include(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                    .SingleOrDefault(sp => sp.IdSubPremises == contract.IdSubPremise);
                return new Address
                {
                    Id = subPremise?.IdSubPremises.ToString(),
                    AddressType = AddressTypes.SubPremise,
                    Text = subPremise?.GetAddress(),
                    IdParents = new Dictionary<string, string>
                    {
                        {"IdStreet", subPremise?.IdPremisesNavigation?.IdBuildingNavigation?.IdStreet },
                        {"IdBuilding", subPremise?.IdPremisesNavigation?.IdBuilding.ToString() },
                        {"IdPremise", subPremise?.IdPremisesNavigation?.IdPremises.ToString() },
                        {"IdSubPremise", subPremise?.IdSubPremises.ToString() }
                    }
                };
            }
            return null;
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
            if (privContract == null)
                return privContract;
            privContract.PrivContractors = registryContext.PrivContractors
                .Include(pc => pc.KinshipNavigation)
                .Where(pc => pc.IdContract == idContract)
                .ToList();
            return privContract;
        }

        internal List<Kinship> Kinships { get => registryContext.Kinships.ToList(); }
        internal List<Executor> Executors { get => registryContext.Executors.ToList(); }


        public void Create(PrivContract contract)
        {
            registryContext.PrivContracts.Add(contract);
            registryContext.SaveChanges();
        }
        public void Edit(PrivContract contract)
        {
            var oldContract = registryContext.PrivContracts
                .Include(pc => pc.PrivContractors)
                .AsNoTracking()
                .SingleOrDefault(pc => pc.IdContract == contract.IdContract);
            foreach (var oldContractor in oldContract.PrivContractors)
            {
                if (!contract.PrivContractors.Select(pc => pc.IdContractor).Contains(oldContractor.IdContractor))
                {
                    registryContext.Entry(oldContractor).Property(p => p.Deleted).IsModified = true;
                    oldContractor.Deleted = true;
                    contract.PrivContractors.Add(oldContractor);
                }
            }
            registryContext.PrivContracts.Update(contract);
            registryContext.SaveChanges();
        }
        public void Delete(int idContract)
        {
            var contract = registryContext.PrivContracts
                .Include(pc => pc.PrivContractors)
                .FirstOrDefault(pc => pc.IdContract == idContract);
            foreach (var contractor in contract.PrivContractors)
                contractor.Deleted = true;
            contract.Deleted = true;
            registryContext.SaveChanges();
        }
    }
}