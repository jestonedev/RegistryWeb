using RegistryDb.Models;
using System.Linq;
using RegistryWeb.ViewOptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using RegistryWeb.ViewOptions.Filter;
using RegistryDb.Models.Entities.Claims;
using RegistryWeb.DataServices;
using RegistryWeb.Enums;
using RegistryServices.DataServices.KumiAccounts;
using System;
using RegistryWeb.ViewModel;
using RegistryServices.DataServices.Claims;

namespace RegistryServices.DataFilterServices
{
    class ClaimsFilterService : AbstractFilterService<Claim, ClaimsFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly AddressesDataService addressesDataService;
        private readonly KumiAccountsDataService kumiAccountsDataService;
        private readonly ClaimsAssignedAccountsDataService assignedAccountsService;

        public ClaimsFilterService(RegistryContext registryContext,
            AddressesDataService addressesDataService,
            KumiAccountsDataService kumiAccountsDataService,
            ClaimsAssignedAccountsDataService assignedAccountsService)
        {
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
            this.kumiAccountsDataService = kumiAccountsDataService;
            this.assignedAccountsService = assignedAccountsService;
        }

        public override IQueryable<Claim> GetQueryFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = PaymentAccountFilter(query, filterOptions);
                query = ClaimFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<Claim> AddressFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty() &&
                string.IsNullOrEmpty(filterOptions.IdStreet) &&
                string.IsNullOrEmpty(filterOptions.House) &&
                string.IsNullOrEmpty(filterOptions.PremisesNum) &&
                string.IsNullOrEmpty(filterOptions.IdRegion) &&
                filterOptions.IdBuilding == null &&
                filterOptions.IdPremises == null &&
                filterOptions.IdSubPremises == null)
                return query;

            IEnumerable<int> idAccountsBks = SearchBksAccouuntsIds(query, filterOptions);
            IEnumerable<int> idAccountsKumi = SearchKumiAccouuntsIds(query, filterOptions);
            query = (from q in query
                     join idAccount in idAccountsBks on q.IdAccount equals idAccount
                     select q).Union(
                    from q in query
                    join idAccount in idAccountsKumi on q.IdAccountKumi equals idAccount
                    select q).Distinct();
            return query;
        }

        private IEnumerable<int> SearchBksAccouuntsIds(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);
            IEnumerable<int> idAccountsBks = new List<int>();

            var bksPremisesAssoc = registryContext.PaymentAccountPremisesAssoc
                .Include(p => p.PremiseNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            var bksSubPremisesAssoc = registryContext.PaymentAccountSubPremisesAssoc
                .Include(sp => sp.SubPremiseNavigation)
                .ThenInclude(p => p.IdPremisesNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);
            var filtered = false;

            if (filterOptions.Address.AddressType == AddressTypes.Street || !string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var streets = filterOptions.Address.AddressType == AddressTypes.Street ? addresses : new List<string> { filterOptions.IdStreet };
                var idPremiseAccounts = bksPremisesAssoc
                    .Where(opa => streets.Contains(opa.PremiseNavigation.IdBuildingNavigation.IdStreet))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = bksSubPremisesAssoc
                    .Where(ospa => streets.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet))
                    .Select(ospa => ospa.IdAccount);
                idAccountsBks = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            else if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                var idPremiseAccounts = bksPremisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = bksSubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation
                    .IdBuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion))
                    .Select(ospa => ospa.IdAccount);
                idAccountsBks = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));
            if ((filterOptions.Address.AddressType == AddressTypes.Building && addressesInt.Any()) || filterOptions.IdBuilding != null)
            {
                if (filterOptions.IdBuilding != null)
                {
                    addressesInt = new List<int> { filterOptions.IdBuilding.Value };
                }
                var idPremiseAccounts = bksPremisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdBuilding))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = bksSubPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding))
                    .Select(ospa => ospa.IdAccount);
                idAccountsBks = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.Premise && addressesInt.Any()) || filterOptions.IdPremises != null)
            {
                if (filterOptions.IdPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdPremises.Value };
                }
                var idPremiseAccounts = bksPremisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdPremises))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = bksSubPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises))
                    .Select(ospa => ospa.IdAccount);
                idAccountsBks = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.SubPremise && addressesInt.Any()) || filterOptions.IdSubPremises != null)
            {
                if (filterOptions.IdSubPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdSubPremises.Value };
                }
                idAccountsBks = bksSubPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdSubPremises))
                    .Select(ospa => ospa.IdAccount);
                filtered = true;
            }

            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var idPremiseAccounts = bksPremisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = bksSubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()))
                    .Select(ospa => ospa.IdAccount);
                if (filtered)
                    idAccountsBks = idAccountsBks.Intersect(idPremiseAccounts.Union(idSubPremiseAccounts));
                else
                    idAccountsBks = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var idPremiseAccounts = bksPremisesAssoc
                    .Where(opa => opa.PremiseNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant()))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = bksSubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant()))
                    .Select(ospa => ospa.IdAccount);
                if (filtered)
                    idAccountsBks = idAccountsBks.Intersect(idPremiseAccounts.Union(idSubPremiseAccounts));
                else
                    idAccountsBks = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }

            return idAccountsBks.ToList();
        }

        private IEnumerable<int> SearchKumiAccouuntsIds(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);
            IEnumerable<int> idAccountsKumi =
                kumiAccountsDataService.GetKumiAccounts(new KumiAccountsFilter
                {
                    Address = filterOptions.Address,
                    IdStreet = filterOptions.IdStreet,
                    IdRegions = filterOptions.IdRegion != null ? new List<string> { filterOptions.IdRegion } : new List<string>(),
                    IdBuilding = filterOptions.IdBuilding,
                    IdPremises = filterOptions.IdPremises,
                    IdSubPremises = filterOptions.IdSubPremises,
                    House = filterOptions.House,
                    PremisesNum = filterOptions.PremisesNum
                }).Select(r => r.IdAccount);
            return idAccountsKumi.ToList();
        }

        private IQueryable<Claim> PaymentAccountFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.Crn))
            {
                query = query.Where(p => p.IdAccountNavigation.Crn.Contains(filterOptions.Crn)
                    || p.IdAccountAdditionalNavigation.Crn.Contains(filterOptions.Crn));
            }
            if (!string.IsNullOrEmpty(filterOptions.Account))
            {
                query = query.Where(p => p.IdAccountNavigation.Account.Contains(filterOptions.Account)
                    || p.IdAccountAdditionalNavigation.Account.Contains(filterOptions.Account)
                    || p.IdAccountKumiNavigation.Account.Contains(filterOptions.Account));
            }
            if (!string.IsNullOrEmpty(filterOptions.RawAddress))
            {
                query = query.Where(p => p.IdAccountNavigation.RawAddress.Contains(filterOptions.RawAddress)
                    || p.IdAccountAdditionalNavigation.RawAddress.Contains(filterOptions.RawAddress));
            }
            return query;
        }

        private IQueryable<Claim> ClaimFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (filterOptions.IdClaim != null && filterOptions.IdClaim != 0)
            {
                query = query.Where(p => p.IdClaim == filterOptions.IdClaim.Value);
            }
            if (filterOptions.IdAccountBks != null)
            {
                var ids = assignedAccountsService.GetAccountIdsWithSameAddress(filterOptions.IdAccountBks.Value);
                query = query.Where(p => ids.Contains(p.IdAccount ?? 0) || p.IdAccount == filterOptions.IdAccountBks || p.IdAccountAdditional == filterOptions.IdAccountBks);
            }
            if (filterOptions.IdAccountKumi != null)
            {
                var infixes = registryContext.GetAddressByAccountIds(new List<int> { filterOptions.IdAccountKumi.Value }).Select(r => r.Infix).ToList();
                var ids = registryContext.GetKumiAccountIdsByAddressInfixes(infixes);
                query = query.Where(p => p.IdAccountKumi != null && ids.Contains(p.IdAccountKumi.Value));
            }
            if (!string.IsNullOrEmpty(filterOptions.Tenant))
            {
                var tenantClaims = registryContext.ClaimPersons.Where(cp => cp.IsClaimer).Select(c => new { c.IdPerson, c.IdClaim, fio = (String.Concat(c.Surname, ' ', c.Name, ' ', c.Patronymic)).ToLower() });
                var tenantsIds = tenantClaims.Where(g => g.fio.Contains(filterOptions.Tenant.ToLower())).Select(g => g.IdClaim).ToList();
                query = query.Where(p => tenantsIds.Contains(p.IdClaim));
            }
            if (filterOptions.AmountTotal != null)
            {
                query = query.Where(p => filterOptions.AmountTotalOp == 1 ?
                    p.AmountTenancy + p.AmountPenalties + p.AmountDgi + p.AmountPadun + p.AmountPkk >= filterOptions.AmountTotal :
                    p.AmountTenancy + p.AmountPenalties + p.AmountDgi + p.AmountPadun + p.AmountPkk <= filterOptions.AmountTotal);
            }
            if (filterOptions.AmountTenancy != null)
            {
                query = query.Where(p => filterOptions.AmountTenancyOp == 1 ?
                    p.AmountTenancy >= filterOptions.AmountTenancy :
                    p.AmountTenancy <= filterOptions.AmountTenancy);
            }
            if (filterOptions.AmountPenalties != null)
            {
                query = query.Where(p => filterOptions.AmountPenaltiesOp == 1 ?
                    p.AmountPenalties >= filterOptions.AmountPenalties :
                    p.AmountPenalties <= filterOptions.AmountPenalties);
            }
            if (filterOptions.AmountDgiPadunPkk != null)
            {
                query = query.Where(p => filterOptions.AmountDgiPadunPkkOp == 1 ?
                    (p.AmountDgi >= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPadun >= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPkk >= filterOptions.AmountDgiPadunPkk) :
                    (p.AmountDgi <= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPadun <= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPkk <= filterOptions.AmountDgiPadunPkk));
            }
            if (filterOptions.AtDate != null)
            {
                query = query.Where(p => p.AtDate == filterOptions.AtDate);
            }
            if (!string.IsNullOrEmpty(filterOptions.CourtOrderNum))
            {
                var idClaims = registryContext.ClaimStates
                    .Where(cs => (cs.IdStateType == 4 || cs.IdStateType == 8) && cs.CourtOrderNum.Contains(filterOptions.CourtOrderNum)).Select(r => r.IdClaim).ToList();
                query = query.Where(p => idClaims.Contains(p.IdClaim));
            }

            if (filterOptions.IdClaimState != null)
            {
                if (filterOptions.IsCurrentState)
                {
                    var maxDateClaimStates =
                        from row in registryContext.ClaimStates
                        group row.IdState by row.IdClaim into gs
                        select new
                        {
                            IdClaim = gs.Key,
                            IdState = gs.Max()
                        };
                    var lastClaimsStates =
                        (from row in registryContext.ClaimStates
                         join maxDateClaimStatesRow in maxDateClaimStates
                         on row.IdState equals maxDateClaimStatesRow.IdState
                         select new
                         {
                             row.IdClaim,
                             row.IdStateType,
                             row.DateStartState,
                             row.ClaimDirectionDate,
                             row.CourtOrderDate,
                             row.ObtainingCourtOrderDate
                         }).ToList();

                    query = from row in query
                            join lastClaimsStatesRow in lastClaimsStates
                            on row.IdClaim equals lastClaimsStatesRow.IdClaim
                            where lastClaimsStatesRow.IdStateType == filterOptions.IdClaimState
                            select row;

                    if (filterOptions.ClaimDirectionDateFrom != null)
                    {
                        query = from row in query
                                join lastClaimsStatesRow in lastClaimsStates
                                on row.IdClaim equals lastClaimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.ClaimDirectionDateOp,
                                    lastClaimsStatesRow.ClaimDirectionDate,
                                    filterOptions.ClaimDirectionDateFrom,
                                    filterOptions.ClaimDirectionDateTo)
                                select row;
                    }

                    if (filterOptions.CourtOrderDateFrom != null)
                    {
                        query = from row in query
                                join lastClaimsStatesRow in lastClaimsStates
                                on row.IdClaim equals lastClaimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.CourtOrderDateOp,
                                    lastClaimsStatesRow.CourtOrderDate,
                                    filterOptions.CourtOrderDateFrom,
                                    filterOptions.CourtOrderDateTo)
                                select row;
                    }

                    if (filterOptions.ObtainingCourtOrderDateFrom != null)
                    {
                        query = from row in query
                                join lastClaimsStatesRow in lastClaimsStates
                                on row.IdClaim equals lastClaimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.ObtainingCourtOrderDateOp,
                                    lastClaimsStatesRow.ObtainingCourtOrderDate,
                                    filterOptions.ObtainingCourtOrderDateFrom,
                                    filterOptions.ObtainingCourtOrderDateTo)
                                select row;
                    }
                }
                else
                {
                    if (filterOptions.ClaimStateDateFrom != null)
                    {
                        query = from row in query
                                join claimsStatesRow in registryContext.ClaimStates
                                on row.IdClaim equals claimsStatesRow.IdClaim
                                where claimsStatesRow.IdStateType == filterOptions.IdClaimState
                                && DateComparison(
                                    filterOptions.ClaimStateDateOp,
                                    claimsStatesRow.DateStartState,
                                    filterOptions.ClaimStateDateFrom,
                                    filterOptions.ClaimStateDateTo)
                                select row;
                    }
                    else
                    {
                        query = from row in query
                                join claimsStatesRow in registryContext.ClaimStates
                                on row.IdClaim equals claimsStatesRow.IdClaim
                                where claimsStatesRow.IdStateType == filterOptions.IdClaimState
                                select row;
                    }

                    if (filterOptions.ClaimDirectionDateFrom != null)
                    {
                        query = from row in query
                                join claimsStatesRow in registryContext.ClaimStates
                                on row.IdClaim equals claimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.ClaimDirectionDateOp,
                                    claimsStatesRow.ClaimDirectionDate,
                                    filterOptions.ClaimDirectionDateFrom,
                                    filterOptions.ClaimDirectionDateTo)
                                select row;
                    }

                    if (filterOptions.CourtOrderDateFrom != null)
                    {
                        query = from row in query
                                join claimsStatesRow in registryContext.ClaimStates
                                on row.IdClaim equals claimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.CourtOrderDateOp,
                                    claimsStatesRow.CourtOrderDate,
                                    filterOptions.CourtOrderDateFrom,
                                    filterOptions.CourtOrderDateTo)
                                select row;
                    }

                    if (filterOptions.ObtainingCourtOrderDateFrom != null)
                    {
                        query = from row in query
                                join claimsStatesRow in registryContext.ClaimStates
                                on row.IdClaim equals claimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.ObtainingCourtOrderDateOp,
                                    claimsStatesRow.ObtainingCourtOrderDate,
                                    filterOptions.ObtainingCourtOrderDateFrom,
                                    filterOptions.ObtainingCourtOrderDateTo)
                                select row;
                    }
                }
            }

            if (filterOptions.ClaimStateDateFrom != null && filterOptions.IdClaimState == null)
            {
                query = (from row in query
                         join claimsStatesRow in registryContext.ClaimStates
                         on row.IdClaim equals claimsStatesRow.IdClaim
                         where DateComparison(
                             filterOptions.ClaimStateDateOp,
                             claimsStatesRow.DateStartState,
                             filterOptions.ClaimStateDateFrom,
                             filterOptions.ClaimStateDateTo)
                         select row).Distinct();
            }

            if (filterOptions.BalanceOutputTotal != null || filterOptions.BalanceOutputTenancy != null ||
                filterOptions.BalanceOutputPenalties != null || filterOptions.BalanceOutputDgiPadunPkk != null)
            {
                var maxDatePayments = from row in registryContext.Payments
                                      group row.Date by row.IdAccount into gs
                                      select new
                                      {
                                          IdAccount = gs.Key,
                                          Date = gs.Max()
                                      };

                var lastPayments = (from row in registryContext.Payments
                                    join maxDatePaymentsRow in maxDatePayments
                                    on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                                    select new
                                    {
                                        row.IdAccount,
                                        row.BalanceOutputTotal,
                                        row.BalanceOutputTenancy,
                                        row.BalanceOutputPenalties,
                                        row.BalanceOutputDgi,
                                        row.BalanceOutputPkk,
                                        row.BalanceOutputPadun,
                                    }).ToList();

                if (filterOptions.BalanceOutputTotal != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputTotalOp == 1 ?
                               lastPaymentsRow.BalanceOutputTotal >= filterOptions.BalanceOutputTotal :
                               lastPaymentsRow.BalanceOutputTotal <= filterOptions.BalanceOutputTotal
                            select row;
                }
                if (filterOptions.BalanceOutputTenancy != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputTenancyOp == 1 ?
                               lastPaymentsRow.BalanceOutputTenancy >= filterOptions.BalanceOutputTenancy :
                               lastPaymentsRow.BalanceOutputTenancy <= filterOptions.BalanceOutputTenancy
                            select row;
                }
                if (filterOptions.BalanceOutputPenalties != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputPenaltiesOp == 1 ?
                               lastPaymentsRow.BalanceOutputPenalties >= filterOptions.BalanceOutputPenalties :
                               lastPaymentsRow.BalanceOutputPenalties <= filterOptions.BalanceOutputPenalties
                            select row;
                }
                if (filterOptions.BalanceOutputDgiPadunPkk != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputDgiPadunPkkOp == 1 ?
                                (lastPaymentsRow.BalanceOutputDgi >= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPadun >= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPkk >= filterOptions.BalanceOutputDgiPadunPkk) :
                                (lastPaymentsRow.BalanceOutputDgi <= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPadun <= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPkk <= filterOptions.BalanceOutputDgiPadunPkk)
                            select row;
                }
            }

            if (filterOptions.ClaimFormStatementSSPDateFrom != null)
            {
                var idClaimsLogs = (from log in registryContext.LogClaimStatementInSpp
                                    where DateComparison(
                                        filterOptions.ClaimFormStatementSSPDateOp,
                                        log.CreateDate,
                                        filterOptions.ClaimFormStatementSSPDateFrom,
                                        filterOptions.ClaimFormStatementSSPDateTo)
                                    select log.IdClaim).ToList();

                query = from row in query
                        where idClaimsLogs.Contains(row.IdClaim)
                        select row;
            }

            if (filterOptions.StatusSending != null)
            {
                var idClaimUin = (from row in registryContext.UinForClaimStatementInSsp
                                  where row.StatusSending == true
                                  select row.IdClaim).ToList();

                switch (filterOptions.StatusSending)
                {
                    case true:
                        query = from row in query
                                where idClaimUin.Contains(row.IdClaim)
                                select row;
                        break;

                    case false:
                        query = from row in query
                                where !idClaimUin.Contains(row.IdClaim)
                                select row;
                        break;
                }
            }
            return query;
        }


        private bool DateComparison(ComparisonSignEnum sign, DateTime? date, DateTime? dateFrom, DateTime? dateTo)
        {
            switch (sign)
            {
                case ComparisonSignEnum.GreaterThanOrEqual:
                    return date >= dateFrom;
                case ComparisonSignEnum.LessThanOrEqual:
                    return date <= dateFrom;
                case ComparisonSignEnum.Equal:
                    return date == dateFrom;
                case ComparisonSignEnum.Between:
                    return date >= dateFrom && date <= dateTo;
            }
            return false;
        }

        public override IQueryable<Claim> GetQueryIncludes(IQueryable<Claim> query)
        {

            return query
                .Include(c => c.ClaimStates)
                .Include(c => c.ClaimPersons)
                .Include(c => c.IdAccountNavigation)
                .Include(c => c.IdAccountAdditionalNavigation)
                .Include(c => c.IdAccountKumiNavigation);
        }

        public override IQueryable<Claim> GetQueryOrder(IQueryable<Claim> query, OrderOptions orderOptions)
        {

            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdClaim")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdClaim);
                else
                    return query.OrderByDescending(p => p.IdClaim);
            }
            if (orderOptions.OrderField == "AtDate")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.AtDate);
                else
                    return query.OrderByDescending(p => p.AtDate);
            }
            if (orderOptions.OrderField == "Address")
            {
                var addresses =
                    registryContext.PaymentAccountPremisesAssoc
                    .Include(r => r.PremiseNavigation)
                    .ThenInclude(r => r.IdBuildingNavigation)
                    .ThenInclude(r => r.IdStreetNavigation)
                    .Select(
                    p => new
                    {
                        p.IdAccount,
                        Address = string.Concat(p.PremiseNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            p.PremiseNavigation.IdBuildingNavigation.House, ", ", p.PremiseNavigation.PremisesNum)
                    })
                .Union(registryContext.PaymentAccountSubPremisesAssoc
                    .Include(r => r.SubPremiseNavigation)
                    .ThenInclude(r => r.IdPremisesNavigation)
                    .ThenInclude(r => r.IdBuildingNavigation)
                    .ThenInclude(r => r.IdStreetNavigation)
                    .Select(
                    sp => new
                    {
                        sp.IdAccount,
                        Address = string.Concat(sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.PremisesNum, ", ", sp.SubPremiseNavigation.SubPremisesNum)
                    }));
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdAccount equals addr.IdAccount into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address
                            select row).Distinct();
                }
                else
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdAccount equals addr.IdAccount into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address descending
                            select row).Distinct();
                }
            }
            return query;
        }
    }
}
