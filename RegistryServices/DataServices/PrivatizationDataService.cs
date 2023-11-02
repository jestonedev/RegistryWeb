using System;
using RegistryDb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using RegistryDb.Models.SqlViews;
using RegistryWeb.Enums;
using RegistryDb.Interfaces;
using RegistryServices.ViewModel.Privatization;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.Privatization;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.DataFilterServices;

namespace RegistryWeb.DataServices
{
    public class PrivatizationDataService : ListDataService<PrivatizationListVM, PrivatizationFilter>
    {
        private readonly IFilterService<PrivContract, PrivatizationFilter> filterService;
        public PrivatizationDataService(RegistryContext registryContext, AddressesDataService addressesDataService,
            FilterServiceFactory<IFilterService<PrivContract, PrivatizationFilter>> filterServiceFactory)
            : base(registryContext, addressesDataService)
        {
            filterService = filterServiceFactory.CreateInstance();
        }

        public override PrivatizationListVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PrivatizationFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }

        public PrivatizationListVM GetViewModel(OrderOptions orderOptions, PageOptions pageOptions, PrivatizationFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var privContract = GetQuery();
            viewModel.PageOptions.TotalRows = privContract.Count();
            var query = privContract;
            query = filterService.GetQueryFilter(privContract, viewModel.FilterOptions);
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            query = filterService.GetQueryIncludes(query);
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.PrivContracts = filterService.GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.PrivContractsAddresses = GetPrivContractsAddresses(viewModel.PrivContracts);
            return viewModel;
        }

        private Dictionary<int, List<Address>> GetPrivContractsAddresses(List<PrivContract> privContracts)
        {
            var addresses = new Dictionary<int, List<Address>>();
            foreach(var contract in privContracts)
            {
                addresses.Add(contract.IdContract, GetContractAddresses(contract));
            }
            return addresses;
        }

        public List<Address> GetContractAddresses(PrivContract contract)
        {
            var result = new List<Address>();
            result.Add(GeAddresseByIds(contract.IdBuilding, contract.IdPremise, contract.IdSubPremise));
            foreach(var additionalEstate in contract.PrivAdditionalEstates)
            {
                result.Add(GeAddresseByIds(additionalEstate.IdBuilding, additionalEstate.IdPremise, additionalEstate.IdSubPremise));
            }
            return result;
        }

        private Address GeAddresseByIds(int? idBuilding, int? idPremises, int? idSubPremises)
        {
            if (idSubPremises != null)
            {
                var subPremise = registryContext.SubPremises
                    .Include(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                    .Include(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                    .SingleOrDefault(sp => sp.IdSubPremises == idSubPremises);
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
            if (idPremises != null)
            {
                var premise = registryContext.Premises
                    .Include(p => p.IdPremisesTypeNavigation)
                    .Include(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                    .SingleOrDefault(p => p.IdPremises == idPremises);
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
            if (idBuilding != null)
            {
                var building = registryContext.Buildings
                    .Include(r => r.IdStreetNavigation)
                    .SingleOrDefault(b => b.IdBuilding == idBuilding);
                return new Address
                {
                    Id = building?.IdBuilding.ToString(),
                    AddressType = AddressTypes.Building,
                    Text = building?.GetAddress(),
                    IdParents = new Dictionary<string, string>
                    {
                        {"IdStreet", building?.IdStreet },
                        {"IdBuilding", building?.IdBuilding.ToString() },
                        {"IdPremise", null },
                        {"IdSubPremise", null }
                    }
                };
            }
            return null;
        }

        private IQueryable<PrivContract> GetQuery()
        {
            return registryContext.PrivContracts
                .Include(r => r.PrivAdditionalEstates)
                .Include(r => r.ExecutorNavigation)
                .Include(r => r.TypeOfProperty)
                .Include(r => r.BuildingNavigation)
                .Include(r => r.PremiseNavigation)
                .Include(r => r.SubPremiseNavigation)
                .AsNoTracking();
        }

        public PrivContract GetPrivContract(int idContract)
        {
            var privContract = registryContext.PrivContracts
                .Include(pc => pc.PrivAdditionalEstates)
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

        public List<Kinship> Kinships { get => registryContext.Kinships.ToList(); }
        public List<Executor> Executors { get => registryContext.Executors.ToList(); }
        public List<KladrRegion> Regions { get => registryContext.KladrRegions.ToList(); }
        public List<KladrStreet> Streets { get => registryContext.KladrStreets.ToList(); }
        public List<PrivTypeOfProperty> TypesOfProperty { get => registryContext.TypesOfProperty.ToList(); }
        public List<PrivEstateOwner> PrivEstateOwners { get => registryContext.PrivEstateOwners.ToList(); }
        public List<SelectableSigner> PrivEstateOwnerSigners { get => registryContext.SelectableSigners.Where(r => r.IdOwner != null).ToList(); }
        public List<PrivRealtor> PrivRealtors { get => registryContext.PrivRealtors.ToList(); }
        public List<DocumentIssuedBy> DocumentsIssuedBy { get => registryContext.DocumentsIssuedBy.ToList(); }
        public List<PrivContractorWarrantTemplate> PrivContractorWarrantTemplates { get => registryContext.PrivContractorWarrantTemplates.ToList(); }

        public void Create(PrivContract contract)
        {
            contract = UpdateEstateids(contract);
            registryContext.PrivContracts.Add(contract);
            registryContext.SaveChanges();
        }
        public void Edit(PrivContract contract)
        {
            contract = UpdateEstateids(contract);
            var oldContract = registryContext.PrivContracts
                .Include(pc => pc.PrivContractors)
                .Include(pc => pc.PrivAdditionalEstates)
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

            foreach (var oldEstate in oldContract.PrivAdditionalEstates)
            {
                if (!contract.PrivAdditionalEstates.Select(pc => pc.IdEstate).Contains(oldEstate.IdEstate))
                {
                    registryContext.Entry(oldEstate).Property(p => p.Deleted).IsModified = true;
                    oldEstate.Deleted = true;
                    contract.PrivAdditionalEstates.Add(oldEstate);
                }
            }

            registryContext.PrivContracts.Update(contract);
            registryContext.SaveChanges();
        }

        private void BindEstateIds(IPrivEstateBinder binder)
        {
            if (binder.IdSubPremise != null)
            {
                var subPremise = registryContext.SubPremises
                    .Include(r => r.IdPremisesNavigation).ThenInclude(r => r.IdBuildingNavigation)
                    .Where(r => r.IdSubPremises == binder.IdSubPremise).AsNoTracking().FirstOrDefault();
                if (subPremise != null)
                {
                    binder.IdPremise = subPremise.IdPremises;
                    binder.IdBuilding = subPremise.IdPremisesNavigation.IdBuilding;
                    binder.IdStreet = subPremise.IdPremisesNavigation.IdBuildingNavigation.IdStreet;
                }
                else
                {
                    binder.IdSubPremise = null;
                }
            }
            else
            if (binder.IdPremise != null)
            {
                var premise = registryContext.Premises.Include(r => r.IdBuildingNavigation)
                    .Where(r => r.IdPremises == binder.IdPremise).AsNoTracking().FirstOrDefault();
                if (premise != null)
                {
                    binder.IdBuilding = premise.IdBuilding;
                    binder.IdStreet = premise.IdBuildingNavigation.IdStreet;
                }
                else
                {
                    binder.IdPremise = null;
                }
            }
            else
            {
                var building = registryContext.Buildings.Where(r => r.IdBuilding == binder.IdBuilding).AsNoTracking().FirstOrDefault();
                if (building != null)
                {
                    binder.IdStreet = building.IdStreet;
                }
            }
        }

        private PrivContract UpdateEstateids(PrivContract contract)
        {
            BindEstateIds(contract);
            foreach(var estate in contract.PrivAdditionalEstates)
            {
                BindEstateIds(estate);
            }
            return contract;
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