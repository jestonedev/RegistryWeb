using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace RegistryWeb.DataServices
{
    public class PaymentAccountsDataService : ListDataService<PaymentsVM, PaymentsFilter>
    {
        private readonly SecurityServices.SecurityService securityService;
        private readonly IConfiguration config;

        public PaymentAccountsDataService(RegistryContext registryContext, SecurityServices.SecurityService securityService, IConfiguration config) : base(registryContext)
        {
            this.securityService = securityService;
            this.config = config;
        }

        public override PaymentsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PaymentsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.Streets = registryContext.KladrStreets;
            return viewModel;
        }

        internal PaymentsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            PaymentsFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var payments = GetQuery();
            viewModel.PageOptions.TotalRows = payments.Count();
            var query = GetQueryFilter(payments, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.Payments = query.ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.Payments);
            viewModel.ClaimsByAddresses = GetClaimsByAddresses(viewModel.Payments);
            return viewModel;
        }

        private IQueryable<Payment> GetQuery()
        {
            var maxDatePayments = from row in registryContext.Payments
                                  group row.Date by row.IdAccount into gs
                                  select new
                                  {
                                      IdAccount = gs.Key,
                                      Date = gs.Max()
                                  };

            return (from row in registryContext.Payments
                    join maxDatePaymentsRow in maxDatePayments
                    on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                    select row).Include(p => p.PaymentAccountNavigation)
                .Include(p => p.PaymentAccountNavigation);
        }

        private IQueryable<Payment> GetQueryIncludes(IQueryable<Payment> query)
        {
            return query
                .Include(p => p.PaymentAccountNavigation);
        }

        private IQueryable<Payment> GetQueryFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = PaymentAccountFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<Payment> AddressFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty() && 
                !string.IsNullOrEmpty(filterOptions.IdStreet) &&
                !string.IsNullOrEmpty(filterOptions.House) &&
                !string.IsNullOrEmpty(filterOptions.PremisesNum) &&
                filterOptions.IdBuilding == null &&
                filterOptions.IdPremises == null &&
                filterOptions.IdSubPremises == null)
                return query;

            var premisesAssoc = registryContext.PaymentAccountPremisesAssoc
                .Include(p => p.PremiseNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            var subPremisesAssoc = registryContext.PaymentAccountSubPremisesAssoc
                .Include(sp => sp.SubPremiseNavigation)
                .ThenInclude(p => p.IdPremisesNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            IEnumerable<int> idAccounts = new List<int>();

            if (filterOptions.Address.AddressType == AddressTypes.Street || !string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var street = filterOptions.Address.AddressType == AddressTypes.Street ? filterOptions.Address.Id : filterOptions.IdStreet;
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.Equals(street))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(street))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
            }
            var id = 0;
            if ((filterOptions.Address.AddressType == AddressTypes.Building && int.TryParse(filterOptions.Address.Id, out id)) || filterOptions.IdBuilding != null)
            {
                if (filterOptions.IdBuilding != null)
                {
                    id = filterOptions.IdBuilding.Value;
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuilding == id)
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding == id)
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
            }
            if ((filterOptions.Address.AddressType == AddressTypes.Premise && int.TryParse(filterOptions.Address.Id, out id)) || filterOptions.IdPremises != null)
            {
                if (filterOptions.IdPremises != null)
                {
                    id = filterOptions.IdPremises.Value;
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdPremises == id)
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises == id)
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
            }
            if ((filterOptions.Address.AddressType == AddressTypes.SubPremise && int.TryParse(filterOptions.Address.Id, out id)) || filterOptions.IdSubPremises != null)
            {
                if (filterOptions.IdSubPremises != null)
                {
                    id = filterOptions.IdSubPremises.Value;
                }
                idAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdSubPremises == id)
                    .Select(ospa => ospa.IdAccount);
            }
            if (idAccounts.Any())
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
            if (!string.IsNullOrEmpty(filterOptions.Crn))
            {
                query = query.Where(p => p.PaymentAccountNavigation.Crn.Contains(filterOptions.Crn));
            }
            if (!string.IsNullOrEmpty(filterOptions.Account))
            {
                query = query.Where(p => p.PaymentAccountNavigation.Account.Contains(filterOptions.Account));
            }
            if (!string.IsNullOrEmpty(filterOptions.Tenant))
            {
                query = query.Where(p => p.Tenant.Contains(filterOptions.Tenant));
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

        private IQueryable<Payment> GetQueryOrder(IQueryable<Payment> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField))
            {
                return  query.OrderByDescending(p => p.Date);
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

        private IQueryable<Payment> GetQueryPage(IQueryable<Payment> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        private Dictionary<int, List<Address>> GetRentObjects(IEnumerable<Payment> payments)
        {
            var ids = payments.Select(r => r.IdAccount);
            var premises = from paRow in registryContext.PaymentAccountPremisesAssoc
                           join premiseRow in registryContext.Premises
                           on paRow.IdPremise equals premiseRow.IdPremises
                           join buildingRow in registryContext.Buildings
                           on premiseRow.IdBuilding equals buildingRow.IdBuilding
                           join streetRow in registryContext.KladrStreets
                           on buildingRow.IdStreet equals streetRow.IdStreet
                           join premiseTypesRow in registryContext.PremisesTypes
                           on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                           where ids.Contains(paRow.IdAccount)
                           select new
                           {
                               paRow.IdAccount,
                               Address = new Address
                               {
                                   AddressType = AddressTypes.Premise,
                                   Id = premiseRow.IdPremises.ToString(),
                                   IdParents = new Dictionary<string, string>
                                       {
                                           { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                           { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() }
                                       },
                                   Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                        premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum)
                               }
                           };
            var subPremises = from paRow in registryContext.PaymentAccountSubPremisesAssoc
                              join subPremiseRow in registryContext.SubPremises
                              on paRow.IdSubPremise equals subPremiseRow.IdSubPremises
                              join premiseRow in registryContext.Premises
                              on subPremiseRow.IdPremises equals premiseRow.IdPremises
                              join buildingRow in registryContext.Buildings
                              on premiseRow.IdBuilding equals buildingRow.IdBuilding
                              join streetRow in registryContext.KladrStreets
                              on buildingRow.IdStreet equals streetRow.IdStreet
                              join premiseTypesRow in registryContext.PremisesTypes
                              on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                              where ids.Contains(paRow.IdAccount)
                              select new
                              {
                                  paRow.IdAccount,
                                  Address = new Address
                                  {
                                      AddressType = AddressTypes.SubPremise,
                                      Id = subPremiseRow.IdSubPremises.ToString(),
                                      IdParents = new Dictionary<string, string>
                                           {
                                              { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                              { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() },
                                              { AddressTypes.Premise.ToString(), premiseRow.IdPremises.ToString() }
                                           },
                                      Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                            premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum, ", к.", subPremiseRow.SubPremisesNum)
                                  }
                              };

            var objects = premises.Union(subPremises).ToList();

            var result =
                objects.GroupBy(r => r.IdAccount)
                .Select(r => new { IdAccount = r.Key, Addresses = r.Select(v => v.Address) })
                .ToDictionary(v => v.IdAccount, v => v.Addresses.ToList());
            return result;
        }

        private Dictionary<int, List<ClaimInfo>> GetClaimsByAddresses(IEnumerable<Payment> payments)
        {
            var ids = payments.Select(r => r.IdAccount);
            var filteredObjects = (from row in
                           (from row in registryContext.PaymentAccountPremisesAssoc
                            where ids.Contains(row.IdAccount)
                            select new
                            {
                                row.IdAccount,
                                Infix = string.Concat("p", row.IdPremise)
                            }).Union(from row in registryContext.PaymentAccountSubPremisesAssoc
                                     where ids.Contains(row.IdAccount)
                                     select new
                                     {
                                         row.IdAccount,
                                         Infix = string.Concat("sp", row.IdSubPremise)
                                     })
                                   orderby row.Infix
                                   group row.Infix by row.IdAccount into gs
                                   select new
                                   {
                                       IdAccount = gs.Key,
                                       AddressCode = string.Join("", gs)
                                   }).AsEnumerable();
            filteredObjects = from paymentsRow in payments
                              join filteredObjectsRow in filteredObjects
                              on paymentsRow.IdAccount equals filteredObjectsRow.IdAccount into fo
                              from foRow in fo.DefaultIfEmpty()
                              select new
                              {
                                  paymentsRow.IdAccount,
                                  AddressCode = foRow != null ? foRow.AddressCode : paymentsRow.PaymentAccountNavigation.RawAddress
                              };

            var allObjects = (from row in (from row in registryContext.PaymentAccountPremisesAssoc
                               select new PaymentAddressInfix
                               {
                                   IdAccount = row.IdAccount,
                                   Infix = string.Concat("p", row.IdPremise)
                               }).Union(from row in registryContext.PaymentAccountSubPremisesAssoc
                                        select new PaymentAddressInfix
                                        {
                                            IdAccount = row.IdAccount,
                                            Infix = string.Concat("sp", row.IdSubPremise)
                                        })
                             orderby row.Infix
                             group row.Infix by row.IdAccount into gs
                             select new
                             {
                                 IdAccount = gs.Key,
                                 AddressCode = string.Join("", gs)
                             }).AsEnumerable();
            allObjects = from paymentsRow in registryContext.PaymentAccounts
                         join allObjectsRow in allObjects
                         on paymentsRow.IdAccount equals allObjectsRow.IdAccount into ao
                         from aoRow in ao.DefaultIfEmpty()
                         select new
                         {
                             paymentsRow.IdAccount,
                             AddressCode = aoRow != null ? aoRow.AddressCode : paymentsRow.RawAddress
                         };
            var accountsAssoc = (from filteredRow in filteredObjects
                                join allRow in allObjects
                                on filteredRow.AddressCode equals allRow.AddressCode
                                select new
                                {
                                    IdAccountFiltered = filteredRow.IdAccount,
                                    IdAccountActual = allRow.IdAccount
                                }).ToList();
            var accountsIds = accountsAssoc.Select(r => r.IdAccountActual);
            var claims = registryContext.Claims.Where(c => accountsIds.Contains(c.IdAccount));
            var claimIds = claims.Select(r => r.IdClaim);

            var claimLastStatesIds = from row in registryContext.ClaimStates
                                     where claimIds.Contains(row.IdClaim)
                                     group row.IdState by row.IdClaim into gs
                                     select new
                                     {
                                         IdClaim = gs.Key,
                                         IdState = gs.Max()
                                     };

            var claimsInfo = from claimRow in claims
                             join claimLastStateRow in claimLastStatesIds
                             on claimRow.IdClaim equals claimLastStateRow.IdClaim into cls
                             from clsRow in cls.DefaultIfEmpty()
                             join claimStateRow in registryContext.ClaimStates.Where(cs => claimIds.Contains(cs.IdClaim))
                             on clsRow.IdState equals claimStateRow.IdState into cs
                             from csRow in cs.DefaultIfEmpty()
                             join claimStateTypeRow in registryContext.ClaimStateTypes
                             on csRow.IdStateType equals claimStateTypeRow.IdStateType into cst
                             from cstRow in cst.DefaultIfEmpty()
                             select new ClaimInfo
                             {
                                 IdClaim = claimRow.IdClaim,
                                 StartDeptPeriod = claimRow.StartDeptPeriod,
                                 EndDeptPeriod = claimRow.EndDeptPeriod,
                                 IdAccount = claimRow.IdAccount,
                                 IdClaimCurrentState = csRow != null ? (int?)csRow.IdStateType : null,
                                 ClaimCurrentState = cstRow != null ? cstRow.StateType : null
                             };

            var result =
                    claimsInfo
                    .Select(c => new ClaimInfo {
                        ClaimCurrentState = c.ClaimCurrentState,
                        IdClaimCurrentState =c.IdClaimCurrentState,
                        IdClaim = c.IdClaim,
                        StartDeptPeriod = c.StartDeptPeriod,
                        EndDeptPeriod = c.EndDeptPeriod,
                        IdAccount = accountsAssoc.First(a => a.IdAccountActual == c.IdAccount).IdAccountFiltered
                    })
                    .GroupBy(r => r.IdAccount)
                    .Select(r => new { IdAccount = r.Key, Claims = r.OrderByDescending(v => v.IdClaim).Select(v => v) })
                    .ToDictionary(v => v.IdAccount, v => v.Claims.ToList());
            return result;
        }
    }
}