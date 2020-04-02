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
    public class BuildingsDataService : ListDataService<BuildingsVM, BuildingsFilter>
    {
        public BuildingsDataService(RegistryContext registryContext) : base(registryContext)
        {           
        }

        public override BuildingsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, BuildingsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }

        public BuildingsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            BuildingsFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.Buildings = GetQueryPage(query, viewModel.PageOptions).ToList();
            return viewModel;
        }

        public IQueryable<Building> GetQuery()
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdStateNavigation)
                .OrderBy(b => b.IdBuilding);
        }

        private IQueryable<Building> GetQueryFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = IdBuildingFilter(query, filterOptions);
                query = IdDecreeFilter(query, filterOptions);
                query = StreetFilter(query, filterOptions);
                query = HouseFilter(query, filterOptions);
                query = FloorsFilter(query, filterOptions);
                query = EntrancesFilter(query, filterOptions);
                query = OwnershipRightFilter(query, filterOptions);
                query = ObjectStateFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<Building> OwnershipRightFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!filterOptions.IsOwnershipRightEmpty())
            {
                var obas = registryContext.OwnershipBuildingsAssoc
                    .Include(oba => oba.OwnershipRightNavigation)
                    .AsTracking();
                if (filterOptions.DateOwnershipRight.HasValue)
                    obas = obas.Where(oba => oba.OwnershipRightNavigation.Date == filterOptions.DateOwnershipRight.Value);
                if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight))
                    obas = obas.Where(oba => oba.OwnershipRightNavigation.Number == filterOptions.NumberOwnershipRight);
                if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Count != 0)
                        obas = obas.Where(oba => filterOptions.IdsOwnershipRightType.Contains(oba.OwnershipRightNavigation.IdOwnershipRightType));
                query = from q in query
                        join idBuilding in obas.Select(oba => oba.IdBuilding).Distinct()
                            on q.IdBuilding equals idBuilding
                        select q;
            }
            return query;
        }

        private IQueryable<Building> AddressFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return query.Where(q => q.IdStreet.Equals(filterOptions.Address.Id));
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                return query.Where(q => q.IdBuilding == id);
            }
            return query;
        }

        private IQueryable<Building> IdBuildingFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IdDecree.HasValue)
            {
                query = query.Where(b => b.IdDecree == filterOptions.IdDecree.Value);
            }
            return query;
        }

        private IQueryable<Building> IdDecreeFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IdDecree.HasValue)
            {
                query = query.Where(b => b.IdDecree == filterOptions.IdDecree.Value);
            }
            return query;
        }

        private IQueryable<Building> StreetFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                query = query.Where(b => b.IdStreet == filterOptions.IdStreet );
            }
            return query;
        }

        private IQueryable<Building> HouseFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                query = query.Where(b => b.House.ToLower() == filterOptions.House.ToLower());
            }
            return query;
        }

        private IQueryable<Building> FloorsFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.Floors.HasValue)
            {
                query = query.Where(b => b.Floors == filterOptions.Floors.Value);
            }
            return query;
        }

        private IQueryable<Building> EntrancesFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.Entrances.HasValue)
            {
                query = query.Where(b => b.Entrances == filterOptions.Entrances.Value);
            }
            return query;
        }

        private IQueryable<Building> ObjectStateFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IdsObjectState != null && filterOptions.IdsObjectState.Count != 0)
            {
                query = query.Where(b => filterOptions.IdsObjectState.Contains(b.IdStateNavigation.IdState));
            }
            return query;
        }

        private IQueryable<Building> GetQueryOrder(IQueryable<Building> query, OrderOptions orderOptions)
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

        public IQueryable<Building> GetQueryPage(IQueryable<Building> query, PageOptions pageOptions)
        {
            return query
            .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
            .Take(pageOptions.SizePage);
        }

        public Building GetBuilding(int idBuilding)
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)
                .Include(b => b.IdStructureTypeNavigation)
                .SingleOrDefault(b => b.IdBuilding == idBuilding);
        }

        public IEnumerable<Building> GetBuildings(List<int> ids)
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Where(b => ids.Contains(b.IdBuilding));
        }

        public IEnumerable<ObjectState> ObjectStates
        {
            get => registryContext.ObjectStates.AsNoTracking();
        }

        public IEnumerable<StructureType> StructureTypes
        {
            get => registryContext.StructureTypes.AsNoTracking();
        }

        public IEnumerable<StructureTypeOverlap> StructureTypeOverlaps
        {
            get => registryContext.StructureTypeOverlaps.AsNoTracking();
        }

        public IEnumerable<KladrStreet> KladrStreets
        {
            get => registryContext.KladrStreets.AsNoTracking();
        }

        public IEnumerable<HeatingType> HeatingTypes
        {
            get => registryContext.HeatingTypes.AsNoTracking();
        }

        public IEnumerable<OwnershipRightType> OwnershipRightTypes
        {
            get => registryContext.OwnershipRightTypes.AsNoTracking();
        }

        public IEnumerable<GovernmentDecree> GovernmentDecrees
        {
            get => registryContext.GovernmentDecrees.AsNoTracking();
        }

        public void Delete(int idBuilding)
        {
        }

        public void Edit(Building newBuilding)
        {
        }
    }
}
