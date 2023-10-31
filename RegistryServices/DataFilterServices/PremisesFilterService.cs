using RegistryDb.Models;
using RegistryWeb.DataServices;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Enums;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryWeb.DataHelpers;

namespace RegistryServices.DataFilterServices
{
    class PremisesFilterService : AbstractFilterService<Premise, PremisesListFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly AddressesDataService addressesDataService;

        public PremisesFilterService(RegistryContext registryContext, AddressesDataService addressesDataService)
        {
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
        }

        public override IQueryable<Premise> GetQueryFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                if (!filterOptions.IsAddressEmpty())
                    query = AddressFilter(query, filterOptions);

                query = IdPremisesFilter(query, filterOptions);
                query = IdBuildingFilter(query, filterOptions);
                query = RegionFilter(query, filterOptions);
                query = StreetFilter(query, filterOptions);
                query = HouseFilter(query, filterOptions);
                query = PremiseNumFilter(query, filterOptions);
                query = FloorsFilter(query, filterOptions);
                query = CadastralNumFilter(query, filterOptions);
                query = FundsFilter(query, filterOptions);
                query = ObjectStateFilter(query, filterOptions);
                query = CommentFilter(query, filterOptions);
                query = DoorKeysFilter(query, filterOptions);
                query = OwnershipRightFilter(query, filterOptions);
                query = RestrictionFilter(query, filterOptions);
                query = DateFilter(query, filterOptions);
                query = PremisesTypeFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<Premise> AddressFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            if (filterOptions.Address.AddressType == AddressTypes.Street)
                return query.Where(q => addresses.Contains(q.IdBuildingNavigation.IdStreet));

            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));

            if (!addressesInt.Any())
                return query;

            if (filterOptions.Address.AddressType == AddressTypes.Building)
                return query.Where(q => addressesInt.Contains(q.IdBuilding));

            if (filterOptions.Address.AddressType == AddressTypes.Premise)
                return query.Where(q => addressesInt.Contains(q.IdPremises));

            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                return from q in query
                       join sp in registryContext.SubPremises
                       on q.IdPremises equals sp.IdPremises
                       where addressesInt.Contains(sp.IdSubPremises)
                       select q;
            }
            return query;
        }

        private IQueryable<Premise> IdPremisesFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdPremise.HasValue)
            {
                query = query.Where(p => p.IdPremises == filterOptions.IdPremise.Value);
            }
            return query;
        }

        private IQueryable<Premise> IdBuildingFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdBuilding.HasValue)
            {
                query = query.Where(b => b.IdBuilding == filterOptions.IdBuilding.Value);
            }
            return query;
        }

        private IQueryable<Premise> RegionFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (string.IsNullOrEmpty(filterOptions.IdStreet) && !string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                query = query.Where(b => b.IdBuildingNavigation.IdStreet.Contains(filterOptions.IdRegion));
            }
            return query;
        }

        private IQueryable<Premise> StreetFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                query = query.Where(b => b.IdBuildingNavigation.IdStreet == filterOptions.IdStreet);
            }
            return query;
        }

        private IQueryable<Premise> HouseFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                query = query.Where(b => b.IdBuildingNavigation.House.ToLower() == filterOptions.House.ToLower());
            }
            return query;
        }

        private IQueryable<Premise> PremiseNumFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                query = query.Where(b => b.PremisesNum.ToLower() == filterOptions.PremisesNum.ToLower());
            }
            return query;
        }

        private IQueryable<Premise> FloorsFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.Floors.HasValue)
            {
                query = query.Where(b => b.Floor == filterOptions.Floors.Value);
            }
            return query;
        }

        private IQueryable<Premise> CadastralNumFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.CadastralNum))
            {
                query = query.Where(b => b.CadastralNum == filterOptions.CadastralNum);
            }
            return query;
        }

        private IQueryable<Premise> FundsFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdFundType != null && filterOptions.IdFundType.Any())
            {
                var maxFunds = from fundRow in registryContext.FundsHistory
                               join fundPremisesRow in registryContext.FundsPremisesAssoc
                               on fundRow.IdFund equals fundPremisesRow.IdFund
                               where fundRow.ExcludeRestrictionDate == null
                               group fundPremisesRow.IdFund by fundPremisesRow.IdPremises into gs
                               select new
                               {
                                   IdPremises = gs.Key,
                                   IdFund = gs.Max()
                               };
                var idPremises = from fundRow in registryContext.FundsHistory
                                 join maxFundRow in maxFunds
                                 on fundRow.IdFund equals maxFundRow.IdFund
                                 join premisesRow in registryContext.Premises
                                 on maxFundRow.IdPremises equals premisesRow.IdPremises
                                 where filterOptions.IdFundType.Contains(fundRow.IdFundType) && ObjectStateHelper.IsMunicipal(premisesRow.IdState)
                                 select maxFundRow.IdPremises;

                var maxFundsSubPremises = from fundRow in registryContext.FundsHistory
                                          join fundSubPremisesRow in registryContext.FundsSubPremisesAssoc
                                          on fundRow.IdFund equals fundSubPremisesRow.IdFund
                                          where fundRow.ExcludeRestrictionDate == null
                                          group fundSubPremisesRow.IdFund by fundSubPremisesRow.IdSubPremises into gs
                                          select new
                                          {
                                              IdSubPremises = gs.Key,
                                              IdFund = gs.Max()
                                          };

                var idPremisesByRooms = (from fundRow in registryContext.FundsHistory
                                         join maxFundRow in maxFundsSubPremises
                                         on fundRow.IdFund equals maxFundRow.IdFund
                                         join subPremiseRow in registryContext.SubPremises
                                         on maxFundRow.IdSubPremises equals subPremiseRow.IdSubPremises
                                         where filterOptions.IdFundType.Contains(fundRow.IdFundType)
                                         select subPremiseRow.IdPremises).Distinct();

                query = from row in query
                        join idPremise in idPremises.Union(idPremisesByRooms)
                        on row.IdPremises equals idPremise
                        select row;
            }
            return query;
        }

        private IQueryable<Premise> ObjectStateFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdsObjectState != null && filterOptions.IdsObjectState.Any())
            {
                var ids = registryContext.SubPremises
                    .Where(sp => filterOptions.IdsObjectState.Contains(sp.IdState))
                    .Select(sp => sp.IdPremises)
                    .Distinct()
                    .ToList();
                query = query.Where(p =>
                    filterOptions.IdsObjectState.Contains(p.IdState) ||
                    ids.Contains(p.IdPremises)
                );
            }
            return query;
        }

        private IQueryable<Premise> CommentFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdsComment != null && filterOptions.IdsComment.Any())
            {
                query = query.Where(p => filterOptions.IdsComment.Contains(p.IdPremisesComment));
            }
            return query;
        }

        private IQueryable<Premise> DoorKeysFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdsDoorKeys != null && filterOptions.IdsDoorKeys.Any())
            {
                if (filterOptions.IdsDoorKeysContains == null || filterOptions.IdsDoorKeysContains.Value)
                    query = query.Where(p => filterOptions.IdsDoorKeys.Contains(p.IdPremisesDoorKeys));
                else
                    query = query.Where(p => !filterOptions.IdsDoorKeys.Contains(p.IdPremisesDoorKeys));
            }
            return query;
        }

        private IQueryable<Premise> OwnershipRightFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) || filterOptions.DateOwnershipRight != null)
            {
                query = (from q in query
                         join obaRow in registryContext.OwnershipBuildingsAssoc
                         on q.IdBuilding equals obaRow.IdBuilding into b
                         from bRow in b.DefaultIfEmpty()
                         join orRow in registryContext.OwnershipRights
                         on bRow.IdOwnershipRight equals orRow.IdOwnershipRight into bor
                         from borRow in bor.DefaultIfEmpty()

                         join opaRow in registryContext.OwnershipPremisesAssoc
                         on q.IdPremises equals opaRow.IdPremises into p
                         from pRow in p.DefaultIfEmpty()
                         join orRow in registryContext.OwnershipRights
                         on pRow.IdOwnershipRight equals orRow.IdOwnershipRight into por
                         from porRow in por.DefaultIfEmpty()

                         where (borRow != null &&
                            ((string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) ||
                             borRow.Number.ToLower() == filterOptions.NumberOwnershipRight.ToLower()) &&
                             (filterOptions.DateOwnershipRight == null || borRow.Date == filterOptions.DateOwnershipRight))) ||
                             (porRow != null &&
                            ((string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) ||
                             porRow.Number.ToLower() == filterOptions.NumberOwnershipRight.ToLower()) &&
                             (filterOptions.DateOwnershipRight == null || porRow.Date == filterOptions.DateOwnershipRight)))
                         select q).Distinct();
            }
            if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any())
            {
                var idBuildings = registryContext.Premises
                .Select(p => p.IdBuilding);

                var buildings = from tbaRow in registryContext.OwnershipBuildingsAssoc
                                join buildingRow in registryContext.Buildings
                                on tbaRow.IdBuilding equals buildingRow.IdBuilding
                                join streetRow in registryContext.KladrStreets
                                on buildingRow.IdStreet equals streetRow.IdStreet
                                where idBuildings.Contains(tbaRow.IdBuilding)
                                select tbaRow;

                var premises = from tpaRow in registryContext.OwnershipPremisesAssoc
                               join premiseRow in registryContext.Premises
                               on tpaRow.IdPremises equals premiseRow.IdPremises
                               join buildingRow in registryContext.Buildings
                               on premiseRow.IdBuilding equals buildingRow.IdBuilding
                               join streetRow in registryContext.KladrStreets
                               on buildingRow.IdStreet equals streetRow.IdStreet
                               join premiseTypesRow in registryContext.PremisesTypes
                               on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                               select tpaRow;

                if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any())
                {
                    var specialOwnershipRightTypeIds = new int[] { 1, 2, 6, 7 };
                    var specialIds = filterOptions.IdsOwnershipRightType.Where(id => specialOwnershipRightTypeIds.Contains(id));
                    var generalIds = filterOptions.IdsOwnershipRightType.Where(id => !specialOwnershipRightTypeIds.Contains(id));
                    var generalOwnershipRightsPremises = from owrRow in registryContext.OwnershipRights
                                                         join pRow in registryContext.OwnershipPremisesAssoc
                                                         on owrRow.IdOwnershipRight equals pRow.IdOwnershipRight
                                                         where generalIds.Contains(owrRow.IdOwnershipRightType)
                                                         select pRow.IdPremises;

                    var specialOwnershipRightsPremises = from owrRow in registryContext.PremisesOwnershipRightCurrent
                                                         where specialIds.Contains(owrRow.IdOwnershipRightType)
                                                         select owrRow.IdPremises;

                    var ownershipRightsPremisesList = generalOwnershipRightsPremises.Union(specialOwnershipRightsPremises).ToList();

                    var generalOwnershipRightsBuildings = from owrRow in registryContext.OwnershipRights
                                                          join bRow in registryContext.OwnershipBuildingsAssoc
                                                          on owrRow.IdOwnershipRight equals bRow.IdOwnershipRight
                                                          where generalIds.Contains(owrRow.IdOwnershipRightType)
                                                          select bRow.IdBuilding;

                    var specialOwnershipRightsBuildings = from owrRow in registryContext.BuildingsOwnershipRightCurrent
                                                          where specialIds.Contains(owrRow.IdOwnershipRightType)
                                                          select owrRow.IdBuilding;

                    var ownershipRightsBuildingsList = generalOwnershipRightsBuildings.Union(specialOwnershipRightsBuildings).ToList(); //

                    var premisesInBuildingsOwnershipRights = (from pRow in registryContext.Premises
                                                              where ownershipRightsBuildingsList.Contains(pRow.IdBuilding)
                                                              select pRow.IdPremises).ToList();

                    ownershipRightsPremisesList = ownershipRightsPremisesList.Union(premisesInBuildingsOwnershipRights).ToList(); //

                    var buildingIds = (from bRow in buildings
                                       where ownershipRightsBuildingsList.Contains(bRow.IdBuilding)
                                       select bRow.IdBuilding).ToList();

                    var premisesIds = (from pRow in premises
                                       where ownershipRightsPremisesList.Contains(pRow.IdPremises)
                                       select pRow.IdPremises).ToList();

                    if (filterOptions.IdsOwnershipRightTypeContains == null || filterOptions.IdsOwnershipRightTypeContains.Value)
                    {
                        query = (from row in query
                                 where buildingIds.Contains(row.IdBuilding) || premisesIds.Contains(row.IdPremises)
                                 select row).Distinct();
                    }
                    else
                    {
                        query = (from row in query
                                 where !buildingIds.Contains(row.IdBuilding) && !premisesIds.Contains(row.IdPremises)
                                 select row).Distinct();
                    }
                }
            }
            return query;
        }

        private IQueryable<Premise> RestrictionFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if ((filterOptions.IdsRestrictionType != null && filterOptions.IdsRestrictionType.Any()) ||
                !string.IsNullOrEmpty(filterOptions.RestrictionNum) || filterOptions.RestrictionDate != null)
            {
                if (filterOptions.IdsRestrictionType != null && filterOptions.IdsRestrictionType.Any())
                {
                    var contains = filterOptions.IdsRestrictionTypeContains == null || filterOptions.IdsRestrictionTypeContains.Value;

                    var idPremises = from rRow in registryContext.Restrictions
                                     join pRow in registryContext.RestrictionPremisesAssoc
                                     on rRow.IdRestriction equals pRow.IdRestriction
                                     where filterOptions.IdsRestrictionType != null &&
                                           filterOptions.IdsRestrictionType.Contains(rRow.IdRestrictionType)
                                     select pRow.IdPremises;

                    var idBuildings = from rRow in registryContext.Restrictions
                                      join bRow in registryContext.RestrictionBuildingsAssoc
                                      on rRow.IdRestriction equals bRow.IdRestriction
                                      where filterOptions.IdsRestrictionType != null &&
                                            filterOptions.IdsRestrictionType.Contains(rRow.IdRestrictionType)
                                      select bRow.IdBuilding;

                    query = (from q in query
                             where contains ? (idPremises.Contains(q.IdPremises) || idBuildings.Contains(q.IdBuilding))
                                : (!idPremises.Contains(q.IdPremises) && !idBuildings.Contains(q.IdBuilding))
                             select q).Distinct();
                }
                if (!string.IsNullOrEmpty(filterOptions.RestrictionNum) || filterOptions.RestrictionDate != null)
                {
                    query = (from q in query
                             join rbaRow in registryContext.RestrictionBuildingsAssoc
                             on q.IdBuilding equals rbaRow.IdBuilding into b
                             from bRow in b.DefaultIfEmpty()
                             join rRow in registryContext.Restrictions
                             on bRow.IdRestriction equals rRow.IdRestriction into bor
                             from borRow in bor.DefaultIfEmpty()

                             join rpaRow in registryContext.RestrictionPremisesAssoc
                             on q.IdPremises equals rpaRow.IdPremises into p
                             from pRow in p.DefaultIfEmpty()
                             join rRow in registryContext.Restrictions
                             on pRow.IdRestriction equals rRow.IdRestriction into por
                             from porRow in por.DefaultIfEmpty()

                             where (borRow != null &&
                                (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                                 borRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                                 (filterOptions.RestrictionDate == null || borRow.Date == filterOptions.RestrictionDate)) ||
                                 (porRow != null &&
                                (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                                 porRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                                 (filterOptions.RestrictionDate == null || porRow.Date == filterOptions.RestrictionDate))
                             select q).Distinct();
                }
            }
            return query;
        }

        private IQueryable<Premise> PremisesTypeFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdsPremisesType != null && filterOptions.IdsPremisesType.Any())
            {
                query = query.Where(p => filterOptions.IdsPremisesType.Contains(p.IdPremisesType));
            }
            return query;
        }

        private IQueryable<Premise> DateFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.StDateOwnershipRight.HasValue && !filterOptions.EndDateOwnershipRight.HasValue)
            {
                var stDate = filterOptions.StDateOwnershipRight.Value.Date;
                query = query.Where(r => r.RegDate != null && r.RegDate >= stDate);
            }
            else if (!filterOptions.StDateOwnershipRight.HasValue && filterOptions.EndDateOwnershipRight.HasValue)
            {
                var fDate = filterOptions.EndDateOwnershipRight.Value.Date.AddDays(1);
                query = query.Where(r => r.RegDate != null && r.RegDate < fDate);
            }
            else if (filterOptions.EndDateOwnershipRight.HasValue && filterOptions.StDateOwnershipRight.HasValue)
            {
                var stDate = filterOptions.StDateOwnershipRight.Value.Date;
                var fDate = filterOptions.EndDateOwnershipRight.Value.Date.AddDays(1);
                query = query.Where(r => r.RegDate != null && r.RegDate >= stDate && r.RegDate < fDate);
            }
            return query;
        }

        public override IQueryable<Premise> GetQueryIncludes(IQueryable<Premise> query)
        {
            return query
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
                .Include(p => p.IdPremisesTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(p => p.FundsPremisesAssoc)
                    .ThenInclude(fpa => fpa.IdFundNavigation)
                        .ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(p => p.SubPremises);
        }

        public override IQueryable<Premise> GetQueryOrder(IQueryable<Premise> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdPremises")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdPremises);
                else
                    return query.OrderByDescending(p => p.IdPremises);
            }
            if (orderOptions.OrderField == "Address")
            {
                var addresses = query.Select(p => new
                {
                    p.IdPremises,
                    Address = string.Concat(p.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            p.IdBuildingNavigation.House, ", ", p.PremisesNum)
                });

                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return from row in query
                           join addr in addresses
                            on row.IdPremises equals addr.IdPremises
                           orderby addr.Address
                           select row;
                }
                else
                {
                    return from row in query
                           join addr in addresses
                            on row.IdPremises equals addr.IdPremises
                           orderby addr.Address descending
                           select row;
                }
            }
            if (orderOptions.OrderField == "ObjectState")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdStateNavigation.StateNeutral);
                else
                    return query.OrderByDescending(p => p.IdStateNavigation.StateNeutral);
            }
            if (orderOptions.OrderField == "CurrentFund")
            {
                var maxFunds = from fundRow in registryContext.FundsHistory
                               join fundPremisesRow in registryContext.FundsPremisesAssoc
                               on fundRow.IdFund equals fundPremisesRow.IdFund
                               where fundRow.ExcludeRestrictionDate == null
                               group fundPremisesRow.IdFund by fundPremisesRow.IdPremises into gs
                               select new
                               {
                                   IdPremises = gs.Key,
                                   IdFund = gs.Max()
                               };
                var funds = from fundRow in registryContext.FundsHistory
                            join maxFundRow in maxFunds
                            on fundRow.IdFund equals maxFundRow.IdFund
                            join fundTypeRow in registryContext.FundTypes
                            on fundRow.IdFundType equals fundTypeRow.IdFundType
                            select new
                            {
                                maxFundRow.IdPremises,
                                fundTypeRow.FundTypeName
                            };
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return from row in query
                           join fund in funds
                            on row.IdPremises equals fund.IdPremises
                           orderby fund.FundTypeName
                           select row;
                }
                else
                {
                    return from row in query
                           join fund in funds
                            on row.IdPremises equals fund.IdPremises
                           orderby fund.FundTypeName descending
                           select row;
                }
            }
            return query;
        }
    }
}
