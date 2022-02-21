using System;
using RegistryDb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities;
using System.Collections.Generic;
using RegistryDb.Models.SqlViews;
using RegistryWeb.Enums;
using RegistryDb.Interfaces;

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

        public PrivatizationListVM GetViewModel(OrderOptions orderOptions, PageOptions pageOptions, PrivatizationFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var privContract = GetQuery();
            viewModel.PageOptions.TotalRows = privContract.Count();
            var query = privContract;
            query = GetQueryFilter(privContract, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            query = GetQueryIncludes(query);
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.PrivContracts = GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.PrivContractsAddresses = GetPrivContractsAddresses(viewModel.PrivContracts);
            return viewModel;
        }

        private IQueryable<PrivContract> GetQueryFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = ModalFilter(query, filterOptions);
                query = OldAddressFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<PrivContract> OldAddressFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (string.IsNullOrEmpty(filterOptions.OldSystemAddress)) return query;
            var addressParts = filterOptions.OldSystemAddress.ToLowerInvariant().Split(' ', 3);
            var streetPart = addressParts[0];
            string housePart = null;
            string premisePart = null;
            if (addressParts.Count() > 1)
            {
                housePart = "д. "+addressParts[1];
            }
            if (addressParts.Count() > 2)
            {
                premisePart = "кв. " + addressParts[2];
            }
            query = query.Where(r => r.PrivAddress != null && r.PrivAddress.Contains(streetPart) 
                && (housePart == null || r.PrivAddress.Contains(housePart))
                && (premisePart == null || r.PrivAddress.Contains(premisePart)));
            return query;
        }

        private IQueryable<PrivContract> AddressFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return
                    from q in query
                    where addresses.Contains(q.IdStreet)
                    select q;
            }

            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a)).ToList();
            if (!addressesInt.Any())
                return query;

            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                return
                    from q in query
                    where q.IdBuilding != null && addressesInt.Contains(q.IdBuilding.Value)
                    select q;
            }
            if (filterOptions.Address.AddressType == AddressTypes.Premise)
            {
                return
                    from q in query
                    where q.IdPremise != null && addressesInt.Contains(q.IdPremise.Value)
                    select q;
            }
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                return
                    from q in query
                    where q.IdSubPremise != null && addressesInt.Contains(q.IdSubPremise.Value)
                    select q;
            }
            return query;
        }

        private IQueryable<PrivContract> ModalFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.RegNumber))
            {
                var regNum = filterOptions.RegNumber.Trim();
                query = from row in query
                        where row.RegNumber.Contains(regNum)
                        select row;
            }
            if (filterOptions.DateIssueCivil != null)
            {
                query = from row in query
                        where row.DateIssueCivil == filterOptions.DateIssueCivil
                        select row;
            }
            if(filterOptions.IsRefusenik)
            {
                query = from row in query
                        where row.IsRefusenik == true
                        select row;
            }

            IEnumerable<int> idsProcess = null;
            if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                var ids = query.Where(r => r.BuildingNavigation != null && r.BuildingNavigation.IdStreet.Contains(filterOptions.IdRegion)).Select(r => r.IdContract)
                    .ToList();
                idsProcess = ids;
            }
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var ids = query.Where(r => r.BuildingNavigation != null && r.BuildingNavigation.IdStreet == filterOptions.IdStreet).Select(r => r.IdContract)
                    .ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var ids = query.Where(r => r.BuildingNavigation != null && r.BuildingNavigation.House == filterOptions.House).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var ids = query.Where(r => r.PremiseNavigation != null && r.PremiseNavigation.PremisesNum == filterOptions.PremisesNum).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.Surname))
            {
                var ids = registryContext.PrivContractors.Where(r => r.Surname != null && r.Surname.Contains(filterOptions.Surname)).Select(r => r.IdContract).ToList();
                idsProcess = ids;
            }
            if (!string.IsNullOrEmpty(filterOptions.Name))
            {
                var ids = registryContext.PrivContractors.Where(r => r.Name != null && r.Name.Contains(filterOptions.Name)).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.Patronymic))
            {
                var ids = registryContext.PrivContractors.Where(r => r.Patronymic != null && r.Patronymic.Contains(filterOptions.Patronymic)).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (filterOptions.BirthDate != null)
            {
                var ids = registryContext.PrivContractors.Where(r => r.DateBirth == filterOptions.BirthDate).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (idsProcess != null)
            {
                query = (from row in query
                         join id in idsProcess
                         on row.IdContract equals id
                         select row).Distinct();
            }
            return query;
        }

        private IQueryable<PrivContract> GetQueryOrder(IQueryable<PrivContract> query, OrderOptions orderOptions)
        {
            return query.OrderByDescending(p => p.IdContract);
        }

        private IQueryable<PrivContract> GetQueryIncludes(IQueryable<PrivContract> query)
        {
            return query
                .Include(r => r.PrivAdditionalEstates)
                .Include(r => r.ExecutorNavigation)
                .Include(r => r.TypeOfProperty)
                .Include(r => r.BuildingNavigation)
                .Include(r => r.PremiseNavigation)
                .Include(r => r.SubPremiseNavigation);
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

        private IQueryable<PrivContract> GetQueryPage(IQueryable<PrivContract> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
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