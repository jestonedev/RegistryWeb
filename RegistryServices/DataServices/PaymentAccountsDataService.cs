using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.Payments;
using RegistryDb.Models.Entities.Acl;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.Payments;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryServices.Enums;

namespace RegistryWeb.DataServices
{
    public class PaymentAccountsDataService : ListDataService<PaymentsVM, PaymentsFilter>
    {
        private readonly SecurityServices.SecurityService securityService;
        private readonly ClaimsDataService claimsDataService;
        private readonly IConfiguration config;

        public PaymentAccountsDataService(RegistryContext registryContext, SecurityServices.SecurityService securityService,
            ClaimsDataService claimsDataService,
            AddressesDataService addressesDataService, IConfiguration config) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
            this.claimsDataService = claimsDataService;
            this.config = config;
        }

        public override PaymentsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PaymentsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.Streets = addressesDataService.KladrStreets;
            return viewModel;
        }

        public PaymentsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            PaymentsFilter filterOptions, out List<int> filteredIds)
        {
            
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "IdRegion", "Region");
            viewModel.KladrStreetsList = new SelectList(addressesDataService.GetKladrStreets(filterOptions?.IdRegion), "IdStreet", "StreetName");
            viewModel.BuildingManagmentOrgsList = new SelectList(registryContext.BuildingManagmentOrgs , "IdOrganization", "Name");

            if (viewModel.FilterOptions.IsEmpty())
            {
                viewModel.PageOptions.Rows = 0;
                viewModel.PageOptions.TotalPages = 0;
                filteredIds = null;
                viewModel.Payments = new List<Payment>();
                viewModel.MonthsList = new Dictionary<int, DateTime>();
                return viewModel;
            }

            var payments = GetQuery();
            viewModel.PageOptions.TotalRows = payments.Count();
            var query = GetQueryFilter(payments, viewModel.FilterOptions);
            
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            filteredIds = query.Select(c => c.IdAccount).ToList();
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.Payments = query.ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.Payments);
            viewModel.ClaimsByAddresses = GetClaimsByAddresses(viewModel.Payments);

            var monthsList = registryContext.Payments
                                .Select(p => p.Date).Distinct()
                                .OrderByDescending(p => p.Date).Take(6)
                                .ToList();
            viewModel.MonthsList = new Dictionary<int, DateTime>();
            for (var i = 0; i < monthsList.Count(); i++)
                viewModel.MonthsList.Add(monthsList[i].Month, monthsList[i].Date);
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
                    select row).Include(p => p.PaymentAccountNavigation);
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
                query = BuildingOrgManagmentFilter(query, filterOptions.IdBuildingManagmentOrg);
                query = PaymentAccountFilter(query, filterOptions);
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

        public Dictionary<int, List<string>> GetTenantsEmails(List<int> ids)
        {
            var emailsDic = new Dictionary<int, List<string>>();
            foreach (var id in ids)
            {
                int? idSubPremise = null;
                var idPremise = registryContext.PaymentAccountPremisesAssoc
                    .Where(papa => papa.IdAccount == id)
                    .FirstOrDefault()
                    ?.IdPremise;
                if (idPremise == null)
                {
                    idSubPremise = registryContext.PaymentAccountSubPremisesAssoc
                        .Where(paspa => paspa.IdAccount == id)
                        .FirstOrDefault()
                        ?.IdSubPremise;
                }

                var processes = registryContext.TenancyActiveProcesses
                    .Where(tap => tap.IdPremises == idPremise && tap.IdSubPremises == idSubPremise);

                var paymentTenant = registryContext.Payments.Where(r => r.IdAccount == id).OrderByDescending(r => r.Date)
                    .Select(r => r.Tenant).FirstOrDefault();

                List<string> emails = new List<string>();
                foreach (var tp in processes)
                {
                    var hasPerson = registryContext.TenancyPersons.Count(r => tp.IdProcess == r.IdProcess && 
                        (r.Surname + " " + r.Name+" "+r.Patronymic).Trim() == paymentTenant) > 0;
                    if (!hasPerson)
                        continue;
                    var curEmails = registryContext.TenancyPersons
                        .Where(per => per.IdProcess == tp.IdProcess && per.Email != null)
                        .Select(per => per.Email)
                        .ToList();
                    emails.AddRange(curEmails);
                }
                emails = emails.Distinct().ToList();
                emailsDic.Add(id, emails);
            }
            return emailsDic;
        }

        public void CreateClaimMass(List<int> accountIds, DateTime atDate)        {
            var payments = GetPaymentsForMassReports(accountIds).ToList();
            foreach(var payment in payments)
            {
                var claim = new Claim {
                    AtDate = atDate,
                    IdAccount = payment.IdAccount,
                    AmountTenancy = payment.BalanceOutputTenancy,
                    AmountPenalties = payment.BalanceOutputPenalties,
                    AmountDgi = payment.BalanceOutputDgi,
                    AmountPadun = payment.BalanceOutputPadun,
                    AmountPkk = payment.BalanceOutputPkk,
                    ClaimStates = new List<ClaimState> {
                        new ClaimState {
                            IdStateType = registryContext.ClaimStateTypes.Where(r => r.IsStartStateType).First().IdStateType,
                            BksRequester = CurrentExecutor?.ExecutorName,
                            DateStartState = DateTime.Now.Date,
                            Executor = CurrentExecutor?.ExecutorName
                        }
                    },
                    ClaimPersons = new List<ClaimPerson>()
                };
                claim.ClaimPersons = claimsDataService.GetClaimPersonsFromTenancy(claim.IdAccount, null);
                if (claim.ClaimPersons.Count == 0)
                {
                    claim.ClaimPersons = claimsDataService.GetClaimPersonsFromPrevClaim(claim.IdAccount, null);
                }

                claimsDataService.Create(claim, new List<Microsoft.AspNetCore.Http.IFormFile>(), LoadPersonsSourceEnum.None);
            }
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
                query = query.Where(q=>q.Date==prevMonth);
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
                switch(filterOptions.IdClaimsBehavior)
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
                claimsInfo = GetClaimsByAddresses(query.ToList());
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
                    } else
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
                    } else
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
                                .Select(p => p.IdAccount )
                            .Union(registryContext.PaymentAccountSubPremisesAssoc
                                .Where(papa => idSubPremises.Contains(papa.IdSubPremise))
                                .Select(p => p.IdAccount))
                            .ToList();

                foreach (var id in idAccounts)
                {
                    var paymentTenant = registryContext.Payments.Where(r => r.IdAccount==id).OrderByDescending(r => r.Date)
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

        private IQueryable<Payment> GetQueryOrder(IQueryable<Payment> query, OrderOptions orderOptions)
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
                           join premiseRow in registryContext.Premises.Include(r => r.IdStateNavigation)
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
                                   ObjectState = premiseRow.IdStateNavigation,
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
                              join subPremiseRow in registryContext.SubPremises.Include(r => r.IdStateNavigation)
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
                                      ObjectState = subPremiseRow.IdStateNavigation,
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

        public List<AccountIdsAssoc> GetAccountIdsAssocs(IEnumerable<Payment> payments)
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
            var result = (from filteredRow in filteredObjects
                                 join allRow in allObjects
                                 on filteredRow.AddressCode equals allRow.AddressCode
                                 select new AccountIdsAssoc
                                 {
                                     IdAccountFiltered = filteredRow.IdAccount,
                                     IdAccountActual = allRow.IdAccount
                                 }).ToList();
            return result;
        }

        public PaymentsAccountTableVM GetPaymentHistoryTable(AclUser user, int idAccount)
        {
            var viewModel = new PaymentsAccountTableVM();
            viewModel.LastPayment = GetQuery().Single(r => r.IdAccount == idAccount);
            viewModel.Payments = (from row in registryContext.Payments.Include(r => r.PaymentAccountNavigation)
                                  where row.IdAccount == idAccount
                                  orderby row.Date
                                  select row).ToList();
            var lastPaymentList = new List<Payment>();
            lastPaymentList.Add(viewModel.LastPayment);
            viewModel.RentObjects = GetRentObjects(lastPaymentList);

            var json = registryContext.PersonalSettings
                .SingleOrDefault(ps => ps.IdUser == user.IdUser)
                ?.PaymentAccauntTableJson;
            if (json != null)
            {
                viewModel.PaymentAccountTableJson =
                    JsonSerializer.Deserialize<PaymentAccountTableJson>(json);
            }
            return viewModel;
        }

        public PaymentsAccountTableVM GetPaymentHistoryRentObjectTable(AclUser user, int idAccount)
        {
            var viewModel = new PaymentsAccountTableVM();
            var lastPayment = GetQuery().Where(r => r.IdAccount == idAccount).ToList();
            var accounts = GetAccountIdsAssocs(lastPayment);
            var accountIds = accounts.Select(r => r.IdAccountActual);
            viewModel.Payments = (from row in registryContext.Payments.Include(r => r.PaymentAccountNavigation)
                                  where accountIds.Contains(row.IdAccount)
                                  orderby row.Date ascending
                                  select row).ToList();
            viewModel.RentObjects = GetRentObjects(lastPayment);
            viewModel.LastPayment = viewModel.Payments.LastOrDefault();

            var json = registryContext.PersonalSettings
                .SingleOrDefault(ps => ps.IdUser == user.IdUser)
                ?.PaymentAccauntTableJson;
            if (json != null)
            {
                viewModel.PaymentAccountTableJson =
                    JsonSerializer.Deserialize<PaymentAccountTableJson>(json);
            }
            return viewModel;
        }

        public bool SavePaymentAccountTableJson(AclUser user, PaymentAccountTableJson vm)
        {
            try
            {
                var json = JsonSerializer.Serialize(vm);
                var personalSetting = registryContext.PersonalSettings
                    .SingleOrDefault(ps => ps.IdUser == user.IdUser);
                if (personalSetting == null)
                {
                    var newPersonalSetting = new PersonalSetting()
                    {
                        IdUser = user.IdUser,
                        PaymentAccauntTableJson = json
                    };
                    registryContext.PersonalSettings.Add(newPersonalSetting);
                }
                else
                {
                    personalSetting.PaymentAccauntTableJson = json;
                }
                registryContext.SaveChanges();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private Dictionary<int, List<ClaimInfo>> GetClaimsByAddresses(IEnumerable<Payment> payments)
        {
            var accountsAssoc = GetAccountIdsAssocs(payments);
            var accountsIds = accountsAssoc.Select(r => r.IdAccountActual);
            var claims = registryContext.Claims.Where(c => c.IdAccount != null && accountsIds.Contains(c.IdAccount.Value));
            var claimIds = claims.Select(r => r.IdClaim);

            var claimLastStatesIds = from row in registryContext.ClaimStates
                                     where claimIds.Contains(row.IdClaim)
                                     group row.IdState by row.IdClaim into gs
                                     select new
                                     {
                                         IdClaim = gs.Key,
                                         IdState = gs.Max()
                                     };

            var claimsInfo = from claimLastStateRow in claimLastStatesIds
                             join claimStateRow in registryContext.ClaimStates.Where(cs => claimIds.Contains(cs.IdClaim))
                             on claimLastStateRow.IdState equals claimStateRow.IdState
                             join claimStateTypeRow in registryContext.ClaimStateTypes
                             on claimStateRow.IdStateType equals claimStateTypeRow.IdStateType
                             select new ClaimInfo
                             {
                                 IdClaim = claimStateRow.IdClaim,
                                 IdClaimCurrentState = claimStateTypeRow.IdStateType,
                                 ClaimCurrentState = claimStateTypeRow.StateType,
                                 ClaimCurrentStateDate = claimStateRow.DateStartState
                             };

            claimsInfo = from claimRow in claims
                         join accountsAssocRow in accountsAssoc
                         on claimRow.IdAccount equals accountsAssocRow.IdAccountActual
                         join claimsInfoRow in claimsInfo
                         on claimRow.IdClaim equals claimsInfoRow.IdClaim into c
                         from cRow in c.DefaultIfEmpty()
                         select new ClaimInfo
                         {
                             IdClaim = claimRow.IdClaim,
                             StartDeptPeriod = claimRow.StartDeptPeriod,
                             EndDeptPeriod = claimRow.EndDeptPeriod,
                             IdAccount = accountsAssocRow.IdAccountFiltered,
                             IdClaimCurrentState = cRow.IdClaimCurrentState,
                             ClaimCurrentState = cRow.ClaimCurrentState,
                             EndedForFilter = claimRow.EndedForFilter,
                             ClaimDescription = claimRow.Description,
                             ClaimCurrentStateDate = cRow.ClaimCurrentStateDate
                         };


            var result =
                    claimsInfo
                    .Select(c => new ClaimInfo {
                        ClaimCurrentState = c.ClaimCurrentState,
                        ClaimCurrentStateDate = c.ClaimCurrentStateDate,
                        IdClaimCurrentState =c.IdClaimCurrentState,
                        IdClaim = c.IdClaim,
                        StartDeptPeriod = c.StartDeptPeriod,
                        EndDeptPeriod = c.EndDeptPeriod,
                        ClaimDescription = c.ClaimDescription,
                        EndedForFilter = c.EndedForFilter,
                        IdAccount = c.IdAccount
                    })
                    .GroupBy(r => r.IdAccount)
                    .Select(r => new { IdAccount = r.Key, Claims = r.OrderByDescending(v => v.IdClaim).Select(v => v) })
                    .ToDictionary(v => v.IdAccount, v => v.Claims.ToList());
            return result;
        }

        public PaymentsVM GetPaymentsViewModelForMassReports(List<int> ids, PageOptions pageOptions)
        {
            var viewModel = InitializeViewModel(null, pageOptions, null);
            var payments = GetPaymentsForMassReports(ids);
            viewModel.PageOptions.TotalRows = payments.Count();
            var count = payments.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Payments = GetQueryPage(payments, viewModel.PageOptions).ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.Payments);
            viewModel.ClaimsByAddresses = GetClaimsByAddresses(viewModel.Payments);
                       
            var monthsList = registryContext.Payments
                            .Select(p => p.Date).Distinct()
                            .OrderByDescending(p => p.Date).Take(6)
                            .ToList();

            viewModel.MonthsList = new Dictionary<int, DateTime>();
            for (var i = 0; i < monthsList.Count(); i++)
                viewModel.MonthsList.Add(monthsList[i].Month, monthsList[i].Date);


            return viewModel;
        }

        public IQueryable<Payment> GetPaymentsForMassReports(List<int> ids)
        {
            return GetQuery().Where(p => ids.Contains(p.IdAccount)).Include(p => p.PaymentAccountNavigation).AsNoTracking();
        }

        public Premise GetPremiseJson(int idPremise)
        {
            var premise = registryContext.Premises
                .Include(p => p.IdStateNavigation)
                .AsNoTracking()
                .SingleOrDefault(p => p.IdPremises == idPremise);
            return premise;
        }

        public List<SelectableSigner> Signers => registryContext.SelectableSigners.ToList();

        public Executor CurrentExecutor
        {
            get
            {
                var userName = securityService.User.UserName.ToLowerInvariant();
                return registryContext.Executors.FirstOrDefault(e => e.ExecutorLogin != null &&
                                e.ExecutorLogin.ToLowerInvariant() == userName);
            }
        }
    }
}