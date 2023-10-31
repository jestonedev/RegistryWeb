using RegistryDb.Models;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryWeb.DataServices;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Enums;
using RegistryWeb.ViewOptions;

namespace RegistryServices.DataFilterServices
{
    class BuildingsFilterService : AbstractFilterService<Building, BuildingsFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly AddressesDataService addressesDataService;

        public BuildingsFilterService(RegistryContext registryContext, AddressesDataService addressesDataService)
        {
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
        }

        public override IQueryable<Building> GetQueryFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = IdBuildingFilter(query, filterOptions);
                query = IdDecreeFilter(query, filterOptions);
                query = RegionFilter(query, filterOptions);
                query = StreetFilter(query, filterOptions);
                query = HouseFilter(query, filterOptions);
                query = FloorsFilter(query, filterOptions);
                query = CadastralNumFilter(query, filterOptions);
                query = StartupDateFilter(query, filterOptions);
                query = EntrancesFilter(query, filterOptions);
                query = OwnershipRightFilter(query, filterOptions);
                query = RestrictionFilter(query, filterOptions);
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
                if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) || filterOptions.DateOwnershipRight != null)
                {
                    if (filterOptions.DateOwnershipRight.HasValue)
                        obas = obas.Where(oba => oba.OwnershipRightNavigation.Date == filterOptions.DateOwnershipRight.Value);
                    if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight))
                        obas = obas.Where(oba => oba.OwnershipRightNavigation.Number == filterOptions.NumberOwnershipRight);

                    query = from q in query
                            join idBuilding in obas.Select(oba => oba.IdBuilding).Distinct()
                                on q.IdBuilding equals idBuilding
                            select q;
                }


                if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Count != 0)
                {

                    var buildings = from tbaRow in registryContext.OwnershipBuildingsAssoc
                                    join buildingRow in registryContext.Buildings
                                    on tbaRow.IdBuilding equals buildingRow.IdBuilding
                                    join streetRow in registryContext.KladrStreets
                                    on buildingRow.IdStreet equals streetRow.IdStreet
                                    select tbaRow;
                    if (filterOptions.DateOwnershipRight.HasValue)
                        buildings = buildings.Where(oba => oba.OwnershipRightNavigation.Date == filterOptions.DateOwnershipRight.Value);
                    if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight))
                        buildings = buildings.Where(oba => oba.OwnershipRightNavigation.Number == filterOptions.NumberOwnershipRight);

                    if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any())
                    {
                        var specialOwnershipRightTypeIds = new int[] { 1, 2, 6, 7 };
                        var specialIds = filterOptions.IdsOwnershipRightType.Where(id => specialOwnershipRightTypeIds.Contains(id));
                        var generalIds = filterOptions.IdsOwnershipRightType.Where(id => !specialOwnershipRightTypeIds.Contains(id));

                        var generalOwnershipRightsBuildings = from owrRow in registryContext.OwnershipRights
                                                              join bRow in registryContext.OwnershipBuildingsAssoc
                                                              on owrRow.IdOwnershipRight equals bRow.IdOwnershipRight
                                                              where generalIds.Contains(owrRow.IdOwnershipRightType)
                                                              select bRow.IdBuilding;

                        var specialOwnershipRightsBuildings = from owrRow in registryContext.BuildingsOwnershipRightCurrent
                                                              where specialIds.Contains(owrRow.IdOwnershipRightType)
                                                              select owrRow.IdBuilding;

                        var ownershipRightsBuildingsList = generalOwnershipRightsBuildings.Union(specialOwnershipRightsBuildings).ToList();

                        var buildingIds = (from bRow in buildings
                                           where ownershipRightsBuildingsList.Contains(bRow.IdBuilding)
                                           select bRow.IdBuilding).ToList();

                        if (filterOptions.IdsOwnershipRightTypeContains == null || filterOptions.IdsOwnershipRightTypeContains.Value)
                        {
                            query = (from row in query
                                     where buildingIds.Contains(row.IdBuilding)
                                     select row).Distinct();
                        }
                        else
                        {
                            query = (from row in query
                                     where !buildingIds.Contains(row.IdBuilding)
                                     select row).Distinct();
                        }
                    }
                }
            }

            return query;
        }

        private IQueryable<Building> RestrictionFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if ((filterOptions.IdsRestrictionType != null && filterOptions.IdsRestrictionType.Any()) ||
                !string.IsNullOrEmpty(filterOptions.RestrictionNum) || filterOptions.RestrictionDate != null)
            {
                query = (from q in query
                         join rbaRow in registryContext.RestrictionBuildingsAssoc
                         on q.IdBuilding equals rbaRow.IdBuilding into b
                         from bRow in b.DefaultIfEmpty()
                         join rRow in registryContext.Restrictions
                         on bRow.IdRestriction equals rRow.IdRestriction into bor
                         from borRow in bor.DefaultIfEmpty()

                         where (borRow != null &&
                            ((filterOptions.IdsRestrictionType == null || !filterOptions.IdsRestrictionType.Any() ||
                            filterOptions.IdsRestrictionType.Contains(borRow.IdRestrictionType)) &&
                            (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                             borRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                             (filterOptions.RestrictionDate == null || borRow.Date == filterOptions.RestrictionDate)))
                         select q).Distinct();
            }
            return query;
        }

        private IQueryable<Building> AddressFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return query.Where(q => addresses.Contains(q.IdStreet));
            }

            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));
                if (!addressesInt.Any())
                    return query;
                return query.Where(q => addressesInt.Contains(q.IdBuilding));
            }
            return query;
        }

        private IQueryable<Building> IdBuildingFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IdBuilding.HasValue)
            {
                query = query.Where(b => b.IdBuilding == filterOptions.IdBuilding.Value);
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

        private IQueryable<Building> RegionFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                query = query.Where(b => b.IdStreet.Contains(filterOptions.IdRegion));
            }
            return query;
        }

        private IQueryable<Building> StreetFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                query = query.Where(b => b.IdStreet == filterOptions.IdStreet);
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

        private IQueryable<Building> CadastralNumFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.CadastralNum))
            {
                query = query.Where(b => b.CadastralNum == filterOptions.CadastralNum);
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

        private IQueryable<Building> StartupDateFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.StartupYear.HasValue)
            {
                query = query.Where(b => b.StartupYear == filterOptions.StartupYear.Value);
            }
            return query;
        }


        public override IQueryable<Building> GetQueryIncludes(IQueryable<Building> query)
        {
            return query
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdStateNavigation)
                .Include(b => b.IdStructureTypeNavigation);
        }

        public override IQueryable<Building> GetQueryOrder(IQueryable<Building> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdBuilding")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(b => b.IdBuilding);
                else
                    return query.OrderByDescending(b => b.IdBuilding);
            }
            if (orderOptions.OrderField == "Address")
            {
                var addresses = query.Select(b => new
                {
                    b.IdBuilding,
                    Address = string.Concat(b.IdStreetNavigation.StreetName, ", ", b.House)
                });

                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return from row in query
                           join addr in addresses
                            on row.IdBuilding equals addr.IdBuilding
                           orderby addr.Address
                           select row;
                }
                else
                {
                    return from row in query
                           join addr in addresses
                            on row.IdBuilding equals addr.IdBuilding
                           orderby addr.Address descending
                           select row;
                }
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
    }
}
