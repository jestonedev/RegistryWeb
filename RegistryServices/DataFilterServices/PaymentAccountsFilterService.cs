using RegistryDb.Models;
using System.Linq;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities.Payments;
using RegistryWeb.ViewOptions.Filter;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using RegistryWeb.Enums;
using RegistryWeb.DataServices;
using System;
using RegistryWeb.ViewModel;
using RegistryServices.DataServices.BksAccounts;

namespace RegistryServices.DataFilterServices
{
    class PaymentAccountsFilterService : AbstractFilterService<Payment, PaymentsFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly AddressesDataService addressesDataService;
        private readonly PaymentAccountsClaimsService claimsService;

        public PaymentAccountsFilterService(RegistryContext registryContext,
            AddressesDataService addressesDataService, PaymentAccountsClaimsService claimsService)
        {
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
            this.claimsService = claimsService;
        }

        public override IQueryable<Payment> GetQueryFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = BuildingOrgManagmentFilter(query, filterOptions.IdBuildingManagmentOrg);
                query = PaymentAccountFilter(query, filterOptions);
            }
            return query;
        }

        public override IQueryable<Payment> GetQueryIncludes(IQueryable<Payment> query)
        {

            return query
                .Include(p => p.PaymentAccountNavigation);
        }

        public override IQueryable<Payment> GetQueryOrder(IQueryable<Payment> query, OrderOptions orderOptions)
        {

            if (string.IsNullOrEmpty(orderOptions.OrderField))
            {
                return query.OrderByDescending(p => p.Date);
            }
            if (orderOptions.OrderField == "Date")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.Date);
                else
                    return query.OrderByDescending(p => p.Date);
            }
            if (orderOptions.OrderField == "Account")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.PaymentAccountNavigation.Account);
                else
                    return query.OrderByDescending(p => p.PaymentAccountNavigation.Account);
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

        private IQueryable<Payment> BuildingOrgManagmentFilter(IQueryable<Payment> query, List<int> idBuildingManagmentOrg)
        {
            if (!idBuildingManagmentOrg.Any()) return query;
            var premisesIds = (from buildingRow in registryContext.Buildings
                               join premiseRow in registryContext.Premises
                               on buildingRow.IdBuilding equals premiseRow.IdBuilding
                               where buildingRow.IdOrganization != null && idBuildingManagmentOrg.Contains(buildingRow.IdOrganization.Value)
                               select premiseRow.IdPremises).ToList();
            var accountsIds = (from accountRow in registryContext.PaymentAccountPremisesAssoc
                               where premisesIds.Contains(accountRow.IdPremise)
                               select accountRow.IdAccount).ToList();
            accountsIds = accountsIds.Union(
                    from accountRow in registryContext.PaymentAccountSubPremisesAssoc
                    join subPremisesRow in registryContext.SubPremises
                    on accountRow.IdSubPremise equals subPremisesRow.IdSubPremises
                    where premisesIds.Contains(subPremisesRow.IdPremises)
                    select accountRow.IdAccount
                ).ToList();
            return query.Where(r => accountsIds.Contains(r.IdAccount));
        }

        private IQueryable<Payment> AddressFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
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

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            var premisesAssoc = registryContext.PaymentAccountPremisesAssoc
                .Include(p => p.PremiseNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            var subPremisesAssoc = registryContext.PaymentAccountSubPremisesAssoc
                .Include(sp => sp.SubPremiseNavigation)
                .ThenInclude(p => p.IdPremisesNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            IEnumerable<int> idAccounts = new List<int>();
            var filtered = false;

            if (filterOptions.Address.AddressType == AddressTypes.Street || !string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var streets = filterOptions.Address.AddressType == AddressTypes.Street ? addresses : new List<string> { filterOptions.IdStreet };
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => streets.Contains(opa.PremiseNavigation.IdBuildingNavigation.IdStreet))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => streets.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            else if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation
                    .IdBuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));

            if ((filterOptions.Address.AddressType == AddressTypes.Building && addressesInt.Any()) || filterOptions.IdBuilding != null)
            {
                if (filterOptions.IdBuilding != null)
                {
                    addressesInt = new List<int> { filterOptions.IdBuilding.Value };
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdBuilding))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.Premise && addressesInt.Any()) || filterOptions.IdPremises != null)
            {
                if (filterOptions.IdPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdPremises.Value };
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdPremises))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.SubPremise && addressesInt.Any()) || filterOptions.IdSubPremises != null)
            {
                if (filterOptions.IdSubPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdSubPremises.Value };
                }
                idAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdSubPremises))
                    .Select(ospa => ospa.IdAccount);
                filtered = true;
            }
            if (filtered)
            {
                query = from q in query
                        join idAccount in idAccounts on q.IdAccount equals idAccount
                        select q;
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()))
                    .Select(ospa => ospa.IdAccount);
                query = from q in query
                        join idAccount in idPremiseAccounts.Union(idSubPremiseAccounts) on q.IdAccount equals idAccount
                        select q;
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant()))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant()))
                    .Select(ospa => ospa.IdAccount);
                query = from q in query
                        join idAccount in idPremiseAccounts.Union(idSubPremiseAccounts) on q.IdAccount equals idAccount
                        select q;
            }
            return query;
        }

        private IQueryable<Payment> PaymentAccountFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.FrontSideAccount))
            {
                query = query.Where(p => p.PaymentAccountNavigation.Account.Contains(filterOptions.FrontSideAccount));
            }
            if (!string.IsNullOrEmpty(filterOptions.Crn))
            {
                query = query.Where(p => p.PaymentAccountNavigation.Crn.Contains(filterOptions.Crn));
            }
            if (!string.IsNullOrEmpty(filterOptions.AccountGisZkh))
            {
                query = query.Where(p => p.PaymentAccountNavigation.AccountGisZkh.Contains(filterOptions.AccountGisZkh));
            }
            if (!string.IsNullOrEmpty(filterOptions.Account))
            {
                query = query.Where(p => p.PaymentAccountNavigation.Account.Contains(filterOptions.Account));
            }
            if (!string.IsNullOrEmpty(filterOptions.Tenant))
            {
                query = query.Where(p => p.Tenant.Contains(filterOptions.Tenant));
            }
            if (filterOptions.Emails)
            {
                query = EmailsFilter(query, filterOptions);
            }
            if (filterOptions.HasPayPrevPeriod)
            {
                var prevMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                query = query.Where(q => q.Date == prevMonth);
            }
            if (!string.IsNullOrEmpty(filterOptions.RawAddress))
            {
                query = query.Where(p => p.PaymentAccountNavigation.RawAddress.Contains(filterOptions.RawAddress));
            }
            query = InputBalanceFilter(query, filterOptions);
            query = ChargingFilter(query, filterOptions);
            query = RecalcFilter(query, filterOptions);
            query = PaymentFilter(query, filterOptions);
            query = OutputBalanceFilter(query, filterOptions);
            query = PresetsFilter(query, filterOptions);
            query = ClaimsBehaviorFilter(query, filterOptions);
            return query;
        }

        private IQueryable<Payment> ClaimsBehaviorFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            if (filterOptions.IdClaimsBehavior != null)
            {
                var filterIdAccounts = registryContext.Claims.Where(r => r.EndedForFilter)
                    .Select(r => r.IdAccount).Distinct().ToList();
                switch (filterOptions.IdClaimsBehavior)
                {
                    case 1:
                        query = query.Where(r => !filterIdAccounts.Contains(r.IdAccount));
                        break;
                    case 2:
                        query = query.Where(r => filterIdAccounts.Contains(r.IdAccount));
                        break;
                }
            }
            return query;
        }

        private IQueryable<Payment> InputBalanceFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            // Входящее сальдо
            if (filterOptions.BalanceInputTotal != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.BalanceInputTotalOp == 1 ?
                        p.BalanceInput >= filterOptions.BalanceInputTotal :
                        p.BalanceInput <= filterOptions.BalanceInputTotal);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.BalanceInputTotalOp == 1 ?
                        p.BalanceInput >= filterOptions.BalanceInputTotal :
                        p.BalanceInput <= filterOptions.BalanceInputTotal)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.BalanceInputTenancy != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.BalanceInputTenancyOp == 1 ?
                        p.BalanceTenancy >= filterOptions.BalanceInputTenancy :
                        p.BalanceTenancy <= filterOptions.BalanceInputTenancy);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.BalanceInputTenancyOp == 1 ?
                        p.BalanceTenancy >= filterOptions.BalanceInputTenancy :
                        p.BalanceTenancy <= filterOptions.BalanceInputTenancy)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.BalanceInputPenalties != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.BalanceInputPenaltiesOp == 1 ?
                        p.BalanceInputPenalties >= filterOptions.BalanceInputPenalties :
                        p.BalanceInputPenalties <= filterOptions.BalanceInputPenalties);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.BalanceInputPenaltiesOp == 1 ?
                        p.BalanceInputPenalties >= filterOptions.BalanceInputPenalties :
                        p.BalanceInputPenalties <= filterOptions.BalanceInputPenalties)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.BalanceInputDgiPadunPkk != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.BalanceInputDgiPadunPkkOp == 1 ?
                        (p.BalanceDgi >= filterOptions.BalanceInputDgiPadunPkk ||
                         p.BalancePadun >= filterOptions.BalanceInputDgiPadunPkk ||
                         p.BalancePkk >= filterOptions.BalanceInputDgiPadunPkk) :
                        (p.BalanceDgi <= filterOptions.BalanceInputDgiPadunPkk ||
                         p.BalancePadun <= filterOptions.BalanceInputDgiPadunPkk ||
                         p.BalancePkk <= filterOptions.BalanceInputDgiPadunPkk));
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.BalanceInputDgiPadunPkkOp == 1 ?
                        (p.BalanceDgi >= filterOptions.BalanceInputDgiPadunPkk ||
                         p.BalancePadun >= filterOptions.BalanceInputDgiPadunPkk ||
                         p.BalancePkk >= filterOptions.BalanceInputDgiPadunPkk) :
                        (p.BalanceDgi <= filterOptions.BalanceInputDgiPadunPkk ||
                         p.BalancePadun <= filterOptions.BalanceInputDgiPadunPkk ||
                         p.BalancePkk <= filterOptions.BalanceInputDgiPadunPkk))).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            return query;
        }

        private IQueryable<Payment> ChargingFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            // Начисление
            if (filterOptions.ChargingTotal != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.ChargingTotalOp == 1 ?
                        p.ChargingTotal >= filterOptions.ChargingTotal :
                        p.ChargingTotal <= filterOptions.ChargingTotal);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.ChargingTotalOp == 1 ?
                        p.ChargingTotal >= filterOptions.ChargingTotal :
                        p.ChargingTotal <= filterOptions.ChargingTotal)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.ChargingTenancy != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.ChargingTenancyOp == 1 ?
                        p.ChargingTenancy >= filterOptions.ChargingTenancy :
                        p.ChargingTenancy <= filterOptions.ChargingTenancy);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.ChargingTenancyOp == 1 ?
                        p.ChargingTenancy >= filterOptions.ChargingTenancy :
                        p.ChargingTenancy <= filterOptions.ChargingTenancy)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.ChargingPenalties != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.ChargingPenaltiesOp == 1 ?
                        p.ChargingPenalties >= filterOptions.ChargingPenalties :
                        p.ChargingPenalties <= filterOptions.ChargingPenalties);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.ChargingPenaltiesOp == 1 ?
                        p.ChargingPenalties >= filterOptions.ChargingPenalties :
                        p.ChargingPenalties <= filterOptions.ChargingPenalties)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.ChargingDgiPadunPkk != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.ChargingDgiPadunPkkOp == 1 ?
                        (p.ChargingDgi >= filterOptions.ChargingDgiPadunPkk ||
                         p.ChargingPadun >= filterOptions.ChargingDgiPadunPkk ||
                         p.ChargingPkk >= filterOptions.ChargingDgiPadunPkk) :
                        (p.ChargingDgi <= filterOptions.ChargingDgiPadunPkk ||
                         p.ChargingPadun <= filterOptions.ChargingDgiPadunPkk ||
                         p.ChargingPkk <= filterOptions.ChargingDgiPadunPkk));
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.ChargingDgiPadunPkkOp == 1 ?
                        (p.ChargingDgi >= filterOptions.ChargingDgiPadunPkk ||
                         p.ChargingPadun >= filterOptions.ChargingDgiPadunPkk ||
                         p.ChargingPkk >= filterOptions.ChargingDgiPadunPkk) :
                        (p.ChargingDgi <= filterOptions.ChargingDgiPadunPkk ||
                         p.ChargingPadun <= filterOptions.ChargingDgiPadunPkk ||
                         p.ChargingPkk <= filterOptions.ChargingDgiPadunPkk))).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            return query;
        }

        private IQueryable<Payment> RecalcFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            // Перерасчет
            if (filterOptions.TransferBalance != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.TransferBalanceOp == 1 ?
                        p.TransferBalance >= filterOptions.TransferBalance :
                        p.TransferBalance <= filterOptions.TransferBalance);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.TransferBalanceOp == 1 ?
                        p.TransferBalance >= filterOptions.TransferBalance :
                        p.TransferBalance <= filterOptions.TransferBalance)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.RecalcTenancy != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.RecalcTenancyOp == 1 ?
                        p.RecalcTenancy >= filterOptions.RecalcTenancy :
                        p.RecalcTenancy <= filterOptions.RecalcTenancy);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.RecalcTenancyOp == 1 ?
                        p.RecalcTenancy >= filterOptions.RecalcTenancy :
                        p.RecalcTenancy <= filterOptions.RecalcTenancy)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.RecalcPenalties != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.RecalcPenaltiesOp == 1 ?
                        p.RecalcPenalties >= filterOptions.RecalcPenalties :
                        p.RecalcPenalties <= filterOptions.RecalcPenalties);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.RecalcPenaltiesOp == 1 ?
                        p.RecalcPenalties >= filterOptions.RecalcPenalties :
                        p.RecalcPenalties <= filterOptions.RecalcPenalties)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.RecalcDgiPadunPkk != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.RecalcDgiPadunPkkOp == 1 ?
                        (p.RecalcDgi >= filterOptions.RecalcDgiPadunPkk ||
                         p.RecalcPadun >= filterOptions.RecalcDgiPadunPkk ||
                         p.RecalcPkk >= filterOptions.RecalcDgiPadunPkk) :
                        (p.RecalcDgi <= filterOptions.RecalcDgiPadunPkk ||
                         p.RecalcPadun <= filterOptions.RecalcDgiPadunPkk ||
                         p.RecalcPkk <= filterOptions.RecalcDgiPadunPkk));
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.RecalcDgiPadunPkkOp == 1 ?
                        (p.RecalcDgi >= filterOptions.RecalcDgiPadunPkk ||
                         p.RecalcPadun >= filterOptions.RecalcDgiPadunPkk ||
                         p.RecalcPkk >= filterOptions.RecalcDgiPadunPkk) :
                        (p.RecalcDgi <= filterOptions.RecalcDgiPadunPkk ||
                         p.RecalcPadun <= filterOptions.RecalcDgiPadunPkk ||
                         p.RecalcPkk <= filterOptions.RecalcDgiPadunPkk))).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            return query;
        }

        private IQueryable<Payment> PaymentFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            // Оплата
            if (filterOptions.PaymentTenancy != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.PaymentTenancyOp == 1 ?
                        p.PaymentTenancy >= filterOptions.PaymentTenancy :
                        p.PaymentTenancy <= filterOptions.PaymentTenancy);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.PaymentTenancyOp == 1 ?
                        p.PaymentTenancy >= filterOptions.PaymentTenancy :
                        p.PaymentTenancy <= filterOptions.PaymentTenancy)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.PaymentPenalties != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.PaymentPenaltiesOp == 1 ?
                        p.PaymentPenalties >= filterOptions.PaymentPenalties :
                        p.PaymentPenalties <= filterOptions.PaymentPenalties);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.PaymentPenaltiesOp == 1 ?
                        p.PaymentPenalties >= filterOptions.PaymentPenalties :
                        p.PaymentPenalties <= filterOptions.PaymentPenalties)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.PaymentDgiPadunPkk != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.PaymentDgiPadunPkkOp == 1 ?
                        (p.PaymentDgi >= filterOptions.PaymentDgiPadunPkk ||
                         p.PaymentPadun >= filterOptions.PaymentDgiPadunPkk ||
                         p.PaymentPkk >= filterOptions.PaymentDgiPadunPkk) :
                        (p.PaymentDgi <= filterOptions.PaymentDgiPadunPkk ||
                         p.PaymentPadun <= filterOptions.PaymentDgiPadunPkk ||
                         p.PaymentPkk <= filterOptions.PaymentDgiPadunPkk));
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.PaymentDgiPadunPkkOp == 1 ?
                        (p.PaymentDgi >= filterOptions.PaymentDgiPadunPkk ||
                         p.PaymentPadun >= filterOptions.PaymentDgiPadunPkk ||
                         p.PaymentPkk >= filterOptions.PaymentDgiPadunPkk) :
                        (p.PaymentDgi <= filterOptions.PaymentDgiPadunPkk ||
                         p.PaymentPadun <= filterOptions.PaymentDgiPadunPkk ||
                         p.PaymentPkk <= filterOptions.PaymentDgiPadunPkk))).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            return query;
        }

        private IQueryable<Payment> OutputBalanceFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            // Исходящее сальдо
            if (filterOptions.BalanceOutputTotal != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.BalanceOutputTotalOp == 1 ?
                        p.BalanceOutputTotal >= filterOptions.BalanceOutputTotal :
                        p.BalanceOutputTotal <= filterOptions.BalanceOutputTotal);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.BalanceOutputTotalOp == 1 ?
                        p.BalanceOutputTotal >= filterOptions.BalanceOutputTotal :
                        p.BalanceOutputTotal <= filterOptions.BalanceOutputTotal)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.BalanceOutputTenancy != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.BalanceOutputTenancyOp == 1 ?
                        p.BalanceOutputTenancy >= filterOptions.BalanceOutputTenancy :
                        p.BalanceOutputTenancy <= filterOptions.BalanceOutputTenancy);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.BalanceOutputTenancyOp == 1 ?
                        p.BalanceOutputTenancy >= filterOptions.BalanceOutputTenancy :
                        p.BalanceOutputTenancy <= filterOptions.BalanceOutputTenancy)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.BalanceOutputPenalties != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.BalanceOutputPenaltiesOp == 1 ?
                        p.BalanceOutputPenalties >= filterOptions.BalanceOutputPenalties :
                        p.BalanceOutputPenalties <= filterOptions.BalanceOutputPenalties);
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.BalanceOutputPenaltiesOp == 1 ?
                        p.BalanceOutputPenalties >= filterOptions.BalanceOutputPenalties :
                        p.BalanceOutputPenalties <= filterOptions.BalanceOutputPenalties)).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            if (filterOptions.BalanceOutputDgiPadunPkk != null)
            {
                if (filterOptions.AtDate == null)
                {
                    query = query.Where(p => filterOptions.BalanceOutputDgiPadunPkkOp == 1 ?
                        (p.BalanceOutputDgi >= filterOptions.BalanceOutputDgiPadunPkk ||
                         p.BalanceOutputPadun >= filterOptions.BalanceOutputDgiPadunPkk ||
                         p.BalanceOutputPkk >= filterOptions.BalanceOutputDgiPadunPkk) :
                        (p.BalanceOutputDgi <= filterOptions.BalanceOutputDgiPadunPkk ||
                         p.BalanceOutputPadun <= filterOptions.BalanceOutputDgiPadunPkk ||
                         p.BalanceOutputPkk <= filterOptions.BalanceOutputDgiPadunPkk));
                }
                else
                {
                    var accountIds = registryContext.Payments.Where(p => p.Date == filterOptions.AtDate &&
                        (filterOptions.BalanceOutputDgiPadunPkkOp == 1 ?
                        (p.BalanceOutputDgi >= filterOptions.BalanceOutputDgiPadunPkk ||
                         p.BalanceOutputPadun >= filterOptions.BalanceOutputDgiPadunPkk ||
                         p.BalanceOutputPkk >= filterOptions.BalanceOutputDgiPadunPkk) :
                        (p.BalanceOutputDgi <= filterOptions.BalanceOutputDgiPadunPkk ||
                         p.BalanceOutputPadun <= filterOptions.BalanceOutputDgiPadunPkk ||
                         p.BalanceOutputPkk <= filterOptions.BalanceOutputDgiPadunPkk))).Select(r => r.IdAccount);
                    query = from row in query
                            join accountId in accountIds
                            on row.IdAccount equals accountId
                            select row;
                }
            }
            return query;
        }

        private IQueryable<Payment> PresetsFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            var claimsInfo = new Dictionary<int, List<ClaimInfo>>();
            if (filterOptions.IdPreset != null)
            {
                claimsInfo = claimsService.GetClaimsByAddresses(query.ToList());
            }
            switch (filterOptions.IdPreset)
            {
                case 1:
                case 2:
                    var ids = new List<int>();
                    foreach (var claimInfo in claimsInfo)
                    {
                        if (claimInfo.Value.Any())
                        {
                            ids.Add(claimInfo.Key);
                        }
                    }
                    if (filterOptions.IdPreset == 1)
                    {
                        // Лицевые счета без исковых работ
                        query = from row in query
                                where !ids.Contains(row.IdAccount)
                                select row;
                    }
                    else
                    {
                        // Лицевые счета с исковыми работами (включая завершенные)
                        query = from row in query
                                where ids.Contains(row.IdAccount)
                                select row;
                    }

                    break;
                case 3:
                case 4:
                    ids = new List<int>();
                    foreach (var claimInfo in claimsInfo)
                    {
                        if (claimInfo.Value.Any())
                        {
                            foreach (var claimStateInfo in claimInfo.Value)
                            {
                                if (claimStateInfo.IdClaimCurrentState != 6)
                                {
                                    ids.Add(claimInfo.Key);
                                    break;
                                }
                            }
                        }
                    }
                    if (filterOptions.IdPreset == 3)
                    {
                        // Лицевые счета с незавершенными исковыми работами
                        query = from row in query
                                where ids.Contains(row.IdAccount)
                                select row;
                    }
                    else
                    {
                        // Лицевые счета, в которых отсутствуют незавершенные исковые работы
                        query = from row in query
                                where !ids.Contains(row.IdAccount)
                                select row;
                    }
                    break;
                case 5:
                    // В суд
                    break;
            }
            return query;
        }

        private IQueryable<Payment> EmailsFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            var idsAccountEmails = new List<int>();

            var anyEmails = registryContext.TenancyPersons
                .Where(per => per.Email != null)
                .Select(per => per.IdProcess)
                .ToList();

            if (anyEmails.Any())
            {
                var idPremises = registryContext.TenancyActiveProcesses
                    .Where(tap => anyEmails.Contains(tap.IdProcess) && tap.IdPremises != null)
                    .Select(tap => tap.IdPremises);

                var idSubPremises = registryContext.TenancyActiveProcesses
                    .Where(tap => anyEmails.Contains(tap.IdProcess) && tap.IdSubPremises != null)
                    .Select(tap => tap.IdSubPremises);

                var idAccounts = registryContext.PaymentAccountPremisesAssoc
                                .Where(papa => idPremises.Contains(papa.IdPremise))
                                .Select(p => p.IdAccount)
                            .Union(registryContext.PaymentAccountSubPremisesAssoc
                                .Where(papa => idSubPremises.Contains(papa.IdSubPremise))
                                .Select(p => p.IdAccount))
                            .ToList();

                foreach (var id in idAccounts)
                {
                    var paymentTenant = registryContext.Payments.Where(r => r.IdAccount == id).OrderByDescending(r => r.Date)
                        .Select(r => r.Tenant).FirstOrDefault();

                    foreach (var tp in anyEmails)
                    {
                        var hasPerson = registryContext.TenancyPersons.Count(r => tp == r.IdProcess &&
                            (r.Surname + " " + r.Name + " " + r.Patronymic).Trim() == paymentTenant) > 0;
                        if (!hasPerson)
                            continue;

                        idsAccountEmails.Add(id);
                    }
                }

                query = query.Where(q => idsAccountEmails.Contains(q.IdAccount));
            }
            return query;
        }

    }
}
