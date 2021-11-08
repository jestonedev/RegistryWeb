using System;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;
using RegistryWeb.Models.SqlViews;

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
            }
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
                query = from row in query
                        where row.RegNumber.Contains(filterOptions.RegNumber)
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
                .Include(r => r.ExecutorNavigation)
                .Include(r => r.TypeOfProperty)
                .Include(r => r.BuildingNavigation)
                .Include(r => r.PremiseNavigation)
                .Include(r => r.SubPremiseNavigation);
        }

        private Dictionary<int, Address> GetPrivContractsAddresses(List<PrivContract> privContracts)
        {
            var addresses = new Dictionary<int, Address>();
            foreach(var contract in privContracts)
            {
                addresses.Add(contract.IdContract, GetAddressRegistry(contract));
            }
            return addresses;
        }

        internal Address GetAddressRegistry(PrivContract contract)
        {
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
            if (contract.IdBuilding != null)
            {
                var building = registryContext.Buildings
                    .Include(r => r.IdStreetNavigation)
                    .SingleOrDefault(b => b.IdBuilding == contract.IdBuilding);
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
                .Include(r => r.ExecutorNavigation)
                .Include(r => r.TypeOfProperty)
                .Include(r => r.BuildingNavigation)
                .Include(r => r.PremiseNavigation)
                .Include(r => r.SubPremiseNavigation).AsNoTracking();
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
        internal List<KladrRegion> Regions { get => registryContext.KladrRegions.ToList(); }
        internal List<KladrStreet> Streets { get => registryContext.KladrStreets.ToList(); }
        internal List<PrivTypeOfProperty> TypesOfProperty { get => registryContext.TypesOfProperty.ToList(); }

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

        private PrivContract UpdateEstateids(PrivContract contract)
        {
            if (contract.IdSubPremise != null)
            {
                var subPremise = registryContext.SubPremises
                    .Include(r => r.IdPremisesNavigation).ThenInclude(r => r.IdBuildingNavigation)
                    .Where(r => r.IdSubPremises == contract.IdSubPremise).AsNoTracking().FirstOrDefault();
                if(subPremise != null)
                {
                    contract.IdPremise = subPremise.IdPremises;
                    contract.IdBuilding = subPremise.IdPremisesNavigation.IdBuilding;
                    contract.IdStreet = subPremise.IdPremisesNavigation.IdBuildingNavigation.IdStreet;
                } else
                {
                    contract.IdSubPremise = null;
                }
            } else
            if (contract.IdPremise != null)
            {
                var premise = registryContext.Premises.Include(r => r.IdBuildingNavigation)
                    .Where(r => r.IdPremises == contract.IdPremise).AsNoTracking().FirstOrDefault();
                if (premise != null)
                {
                    contract.IdBuilding = premise.IdBuilding;
                    contract.IdStreet = premise.IdBuildingNavigation.IdStreet;
                }
                else
                {
                    contract.IdPremise = null;
                }
            } else
            if (contract.IdBuilding != null)
            {
                var building = registryContext.Buildings.Where(r => r.IdBuilding == contract.IdBuilding).AsNoTracking().FirstOrDefault();
                if (building != null)
                {
                    contract.IdStreet = building.IdStreet;
                }
                else
                {
                    contract.IdBuilding = null;
                }
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