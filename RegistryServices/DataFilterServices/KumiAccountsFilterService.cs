using RegistryDb.Models;
using RegistryWeb.DataServices;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Enums;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.DataServices.KumiAccounts;

namespace RegistryServices.DataFilterServices
{
    class KumiAccountsFilterService : AbstractFilterService<KumiAccount, KumiAccountsFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly AddressesDataService addressesDataService;
        private readonly KumiAccountsClaimsService claimsService;

        public KumiAccountsFilterService(RegistryContext registryContext, AddressesDataService addressesDataService,
            KumiAccountsClaimsService claimsService)
        {
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
            this.claimsService = claimsService;
        }

        public override IQueryable<KumiAccount> GetQueryFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = AccountFilter(query, filterOptions);
                query = BalanceFilter(query, filterOptions);
                query = ClaimsBehaviorFilter(query, filterOptions);
                query = EmailsFilter(query, filterOptions);
                query = PresetsFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<KumiAccount> AddressFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty() &&
                string.IsNullOrEmpty(filterOptions.IdStreet) &&
                string.IsNullOrEmpty(filterOptions.House) &&
                string.IsNullOrEmpty(filterOptions.PremisesNum) &&
                string.IsNullOrEmpty(filterOptions.PostIndex) &&
                (filterOptions.IdRegions == null || !filterOptions.IdRegions.Any()) &&
                filterOptions.IdBuilding == null &&
                filterOptions.IdPremises == null &&
                filterOptions.IdSubPremises == null)
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            IEnumerable<int> idAccounts = new List<int>();
            var filtered = false;

            if (filterOptions.Address.AddressType == AddressTypes.Street || !string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var streets = filterOptions.Address.AddressType == AddressTypes.Street ? addresses : new List<string> { filterOptions.IdStreet };
                var infixes = streets.Select(r => string.Concat("s", r));

                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                filtered = true;
            }
            else
            if (filterOptions.IdRegions != null && filterOptions.IdRegions.Any())
            {
                var ids = new List<int>();
                foreach (var idRegion in filterOptions.IdRegions)
                {
                    var infix = "s" + idRegion.ToString();
                    ids.AddRange(registryContext.GetKumiAccountIdsByAddressInfixes(new List<string> { infix }));
                }
                idAccounts = ids;
                filtered = true;
            }
            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));

            if ((filterOptions.Address.AddressType == AddressTypes.Building && addressesInt.Any()) || filterOptions.IdBuilding != null)
            {
                if (filterOptions.IdBuilding != null)
                {
                    addressesInt = new List<int> { filterOptions.IdBuilding.Value };
                }

                var infixes = from buildingBow in registryContext.Buildings
                              where addressesInt.Contains(buildingBow.IdBuilding)
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString());
                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.Premise && addressesInt.Any()) || filterOptions.IdPremises != null)
            {
                if (filterOptions.IdPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdPremises.Value };
                }

                var infixes = from buildingBow in registryContext.Buildings
                              join premisesRow in registryContext.Premises
                              on buildingBow.IdBuilding equals premisesRow.IdBuilding
                              where addressesInt.Contains(premisesRow.IdPremises)
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString(), "p", premisesRow.IdPremises.ToString());
                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.SubPremise && addressesInt.Any()) || filterOptions.IdSubPremises != null)
            {
                if (filterOptions.IdSubPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdSubPremises.Value };
                }

                var infixes = from buildingBow in registryContext.Buildings
                              join premisesRow in registryContext.Premises
                              on buildingBow.IdBuilding equals premisesRow.IdBuilding
                              join subPremisesRow in registryContext.SubPremises
                              on premisesRow.IdPremises equals subPremisesRow.IdPremises
                              where addressesInt.Contains(subPremisesRow.IdSubPremises)
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString(), "p",
                                premisesRow.IdPremises.ToString(), "sp", subPremisesRow.IdSubPremises.ToString());
                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                filtered = true;
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var infixes = from buildingBow in registryContext.Buildings
                              where buildingBow.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant())
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString());
                var idAccountsBuf = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                if (filtered)
                    idAccounts = idAccounts.Intersect(idAccountsBuf);
                else
                    idAccounts = idAccountsBuf;
                filtered = true;

            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var infixes = from buildingBow in registryContext.Buildings
                              join premisesRow in registryContext.Premises
                              on buildingBow.IdBuilding equals premisesRow.IdBuilding
                              where premisesRow.PremisesNum.ToLower().Equals(filterOptions.PremisesNum.ToLowerInvariant())
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString(), "p", premisesRow.IdPremises.ToString());
                var idAccountsBuf = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                if (filtered)
                    idAccounts = idAccounts.Intersect(idAccountsBuf);
                else
                    idAccounts = idAccountsBuf;
                filtered = true;
            }
            if (!string.IsNullOrEmpty(filterOptions.PostIndex))
            {
                var infixes = from buildingBow in registryContext.Buildings
                              where buildingBow.PostIndex != null && buildingBow.PostIndex.Equals(filterOptions.PostIndex)
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString());
                var idAccountsBuf = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                if (filtered)
                    idAccounts = idAccounts.Intersect(idAccountsBuf);
                else
                    idAccounts = idAccountsBuf;
                filtered = true;
            }
            if (filtered)
            {
                query = from q in query
                        where idAccounts.Contains(q.IdAccount)
                        select q;
            }
            return query;
        }

        private IQueryable<KumiAccount> AccountFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.IdsAccount != null && filterOptions.IdsAccount.Any())
            {
                query = query.Where(a => filterOptions.IdsAccount.Contains(a.IdAccount));
            }
            if (!string.IsNullOrEmpty(filterOptions.FrontSideAccount))
            {
                query = query.Where(a => a.Account.Contains(filterOptions.FrontSideAccount));
            }
            if (!string.IsNullOrEmpty(filterOptions.AccountGisZkh))
            {
                query = query.Where(a => a.AccountGisZkh.Contains(filterOptions.AccountGisZkh));
            }
            if (!string.IsNullOrEmpty(filterOptions.Account))
            {
                query = query.Where(a => a.Account.Contains(filterOptions.Account));
            }
            if (filterOptions.IdsAccountState != null && filterOptions.IdsAccountState.Any())
            {
                query = query.Where(a => filterOptions.IdsAccountState.Contains(a.IdState));
            }
            if (!string.IsNullOrEmpty(filterOptions.Tenant))
            {
                var tenantParts = filterOptions.Tenant.Split(' ', 3);
                var surname = tenantParts[0].ToLowerInvariant();
                var tenancyPersons = registryContext.TenancyPersons.Include(tp => tp.IdProcessNavigation).ThenInclude(tp => tp.AccountsTenancyProcessesAssoc)
                    .Where(tp => tp.IdProcessNavigation.AccountsTenancyProcessesAssoc != null &&
                    tp.IdProcessNavigation.AccountsTenancyProcessesAssoc.Count() > 0 && tp.Surname.Contains(surname));
                if (tenantParts.Length > 1)
                {
                    var name = tenantParts[1].ToLowerInvariant();
                    tenancyPersons = tenancyPersons.Where(tp => tp.Name.Contains(name));
                }
                if (tenantParts.Length > 2)
                {
                    var patronymic = tenantParts[2].ToLowerInvariant();
                    tenancyPersons = tenancyPersons.Where(tp => tp.Patronymic.Contains(patronymic));
                }

                var idAccounts = tenancyPersons.SelectMany(tp => tp.IdProcessNavigation.AccountsTenancyProcessesAssoc.Select(r => r.IdAccount)).Distinct().ToList();

                query = query.Where(a => idAccounts.Contains(a.IdAccount));
            }
            return query;
        }

        private IQueryable<KumiAccount> BalanceFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.IsBalanceEmpty()) return query;

            var charges = new List<KumiCharge>();

            if (filterOptions.AtDate == null)
            {
                var lastChargesIds =
                          (from cRow in registryContext.KumiCharges
                           group cRow by cRow.IdAccount into gs
                           select gs.Max(r => r.IdCharge)).ToList();
                charges = (from cRow in registryContext.KumiCharges
                           where lastChargesIds.Contains(cRow.IdCharge)
                           select cRow).ToList();
            }
            else
            {
                charges = registryContext.KumiCharges.Where(r => r.EndDate == filterOptions.AtDate).ToList();
            }

            List<int> resultAIds = null;
            // Input balance
            if (filterOptions.BalanceInputTotal != null)
            {
                var aIds = charges.Where(p => filterOptions.BalanceInputTotalOp == 1 ?
                        p.InputTenancy + p.InputPenalty >= filterOptions.BalanceInputTotal :
                        p.InputTenancy + p.InputPenalty <= filterOptions.BalanceInputTotal).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.BalanceInputTenancy != null)
            {
                var aIds = charges.Where(p => filterOptions.BalanceInputTenancyOp == 1 ?
                        p.InputTenancy >= filterOptions.BalanceInputTenancy :
                        p.InputTenancy <= filterOptions.BalanceInputTenancy).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.BalanceInputPenalties != null)
            {
                var aIds = charges.Where(p => filterOptions.BalanceInputPenaltiesOp == 1 ?
                        p.InputPenalty >= filterOptions.BalanceInputPenalties :
                        p.InputPenalty <= filterOptions.BalanceInputPenalties).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.BalanceInputDgiPadunPkk != null)
            {
                var aIds = charges.Where(p => filterOptions.BalanceInputDgiPadunPkkOp == 1 ?
                        (p.InputDgi >= filterOptions.BalanceInputDgiPadunPkk || p.InputPkk >= filterOptions.BalanceInputDgiPadunPkk || p.InputPadun >= filterOptions.BalanceInputDgiPadunPkk) :
                        (p.InputDgi <= filterOptions.BalanceInputDgiPadunPkk || p.InputPkk <= filterOptions.BalanceInputDgiPadunPkk || p.InputPadun <= filterOptions.BalanceInputDgiPadunPkk)).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            // Output balance
            if (filterOptions.BalanceOutputTotal != null)
            {
                var aIds = charges.Where(p => filterOptions.BalanceOutputTotalOp == 1 ?
                        p.OutputTenancy + p.OutputPenalty >= filterOptions.BalanceOutputTotal :
                        p.OutputTenancy + p.OutputPenalty <= filterOptions.BalanceOutputTotal).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.BalanceOutputTenancy != null)
            {
                var aIds = charges.Where(p => filterOptions.BalanceOutputTenancyOp == 1 ?
                        p.OutputTenancy >= filterOptions.BalanceOutputTenancy :
                        p.OutputTenancy <= filterOptions.BalanceOutputTenancy).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.BalanceOutputPenalties != null)
            {
                var aIds = charges.Where(p => filterOptions.BalanceOutputPenaltiesOp == 1 ?
                        p.OutputPenalty >= filterOptions.BalanceOutputPenalties :
                        p.OutputPenalty <= filterOptions.BalanceOutputPenalties).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.BalanceOutputDgiPadunPkk != null)
            {
                var aIds = charges.Where(p => filterOptions.BalanceOutputDgiPadunPkkOp == 1 ?
                        (p.OutputDgi >= filterOptions.BalanceOutputDgiPadunPkk || p.OutputPkk >= filterOptions.BalanceOutputDgiPadunPkk || p.OutputPadun >= filterOptions.BalanceOutputDgiPadunPkk) :
                        (p.OutputDgi <= filterOptions.BalanceOutputDgiPadunPkk || p.OutputPkk <= filterOptions.BalanceOutputDgiPadunPkk || p.OutputPadun <= filterOptions.BalanceOutputDgiPadunPkk)).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            // Charging
            if (filterOptions.ChargingTotal != null)
            {
                var aIds = charges.Where(p => filterOptions.ChargingTotalOp == 1 ?
                        p.ChargeTenancy + p.ChargePenalty >= filterOptions.ChargingTotal :
                        p.ChargeTenancy + p.ChargePenalty <= filterOptions.ChargingTotal).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.ChargingTenancy != null)
            {
                var aIds = charges.Where(p => filterOptions.ChargingTenancyOp == 1 ?
                        p.ChargeTenancy >= filterOptions.ChargingTenancy :
                        p.ChargeTenancy <= filterOptions.ChargingTenancy).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.ChargingPenalties != null)
            {
                var aIds = charges.Where(p => filterOptions.ChargingPenaltiesOp == 1 ?
                        p.ChargePenalty >= filterOptions.ChargingPenalties :
                        p.ChargePenalty <= filterOptions.ChargingPenalties).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.ChargingDgiPadunPkk != null)
            {
                var aIds = charges.Where(p => filterOptions.ChargingDgiPadunPkkOp == 1 ?
                        (p.ChargeDgi >= filterOptions.ChargingDgiPadunPkk || p.ChargePkk >= filterOptions.ChargingDgiPadunPkk || p.ChargePadun >= filterOptions.ChargingDgiPadunPkk) :
                        (p.ChargeDgi <= filterOptions.ChargingDgiPadunPkk || p.ChargePkk <= filterOptions.ChargingDgiPadunPkk || p.ChargePadun <= filterOptions.ChargingDgiPadunPkk)).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            // Payment
            if (filterOptions.PaymentTotal != null)
            {
                var aIds = charges.Where(p => filterOptions.PaymentTotalOp == 1 ?
                        p.PaymentTenancy + p.PaymentPenalty >= filterOptions.PaymentTotal :
                        p.PaymentTenancy + p.PaymentPenalty <= filterOptions.PaymentTotal).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.PaymentTenancy != null)
            {
                var aIds = charges.Where(p => filterOptions.PaymentTenancyOp == 1 ?
                        p.PaymentTenancy >= filterOptions.PaymentTenancy :
                        p.PaymentTenancy <= filterOptions.PaymentTenancy).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.PaymentPenalties != null)
            {
                var aIds = charges.Where(p => filterOptions.PaymentPenaltiesOp == 1 ?
                        p.PaymentPenalty >= filterOptions.PaymentPenalties :
                        p.PaymentPenalty <= filterOptions.PaymentPenalties).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (filterOptions.PaymentDgiPadunPkk != null)
            {
                var aIds = charges.Where(p => filterOptions.PaymentDgiPadunPkkOp == 1 ?
                        (p.PaymentDgi >= filterOptions.PaymentDgiPadunPkk || p.PaymentPkk >= filterOptions.PaymentDgiPadunPkk || p.PaymentPadun >= filterOptions.PaymentDgiPadunPkk) :
                        (p.PaymentDgi <= filterOptions.PaymentDgiPadunPkk || p.PaymentPkk <= filterOptions.PaymentDgiPadunPkk || p.PaymentPadun <= filterOptions.PaymentDgiPadunPkk)).Select(r => r.IdAccount);
                if (resultAIds == null)
                    resultAIds = aIds.ToList();
                else
                    resultAIds = resultAIds.Intersect(aIds).ToList();
            }

            if (resultAIds != null)
            {
                query = query.Where(r => resultAIds.Contains(r.IdAccount));
            }

            return query;
        }

        private IQueryable<KumiAccount> ClaimsBehaviorFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.IdClaimsBehavior != null)
            {
                var filterIdAccounts = registryContext.Claims.Where(r => r.EndedForFilter)
                    .Select(r => r.IdAccountKumi).Distinct().ToList();
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

        private IQueryable<KumiAccount> EmailsFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (!filterOptions.Emails)
            {
                return query;
            }
            var idAccounts = registryContext.GetAccountIdsWithEmail().Select(r => r.IdAccount);


            query = query.Where(r => idAccounts.Contains(r.IdAccount));
            return query;
        }

        private IQueryable<KumiAccount> PresetsFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.IdPreset == null)
            {
                return query;
            }

            switch (filterOptions.IdPreset)
            {
                case 1:
                case 2:
                    var ids = registryContext.KumiAccounts.Include(a => a.Claims)
                        .Where(a => a.Claims.Any()).Select(a => a.IdAccount).ToList();
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
                    var claimsInfo = claimsService.GetClaimsInfo(query.ToList());
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
                    // Лицевые счета без привязки к найму
                    query = from row in query
                            where !row.AccountsTenancyProcessesAssoc.Any()
                            select row;
                    break;
                case 6:
                case 7:
                    var idProcessWithTenants = (from row in registryContext.TenancyPersons
                                                where row.ExcludeDate == null || row.ExcludeDate > DateTime.Now
                                                select row.IdProcess).Distinct();

                    var actualAccountIds = (from row in registryContext.TenancyProcesses.Include(r => r.AccountsTenancyProcessesAssoc)
                                            join idProcess in idProcessWithTenants
                                            on row.IdProcess equals idProcess
                                            where (row.RegistrationNum == null || !row.RegistrationNum.EndsWith("н")) && row.AccountsTenancyProcessesAssoc.Count() > 0
                                            select row).SelectMany(r => r.AccountsTenancyProcessesAssoc.Select(atpa => atpa.IdAccount)).ToList();
                    if (filterOptions.IdPreset == 6)
                    {
                        // Действующие лицевые счета без действующих наймов
                        query = from row in query
                                where row.AccountsTenancyProcessesAssoc.Any() && row.IdState == 1 && !actualAccountIds.Contains(row.IdAccount)
                                select row;
                    }
                    else
                    {
                        // Аннулированные лицевые счета с действующими наймами
                        query = from row in query
                                where row.AccountsTenancyProcessesAssoc.Any() && row.IdState == 2 && actualAccountIds.Contains(row.IdAccount)
                                select row;
                    }
                    break;
                case 8:
                    query = from row in query
                            where row.RecalcMarker == 1
                            select row;
                    break;
                case 9:
                    var lastChargesIds =
                          (from cRow in registryContext.KumiCharges
                           group cRow by cRow.IdAccount into gs
                           select gs.Max(r => r.IdCharge)).ToList();
                    var charges = (from cRow in registryContext.KumiCharges
                                   where lastChargesIds.Contains(cRow.IdCharge)
                                   select cRow);

                    var currentPeriodEndDate = DateTime.Now.Date;
                    currentPeriodEndDate = currentPeriodEndDate.AddDays(-currentPeriodEndDate.Day + 1).AddMonths(1).AddDays(-1);
                    var nullChargesAccountIds = charges.Where(r => r.EndDate != currentPeriodEndDate || r.ChargeTenancy == 0).Select(r => r.IdAccount).ToList();
                    query = from row in query
                            where row.IdState == 1 && (row.LastCalcDate != currentPeriodEndDate || nullChargesAccountIds.Contains(row.IdAccount))
                            select row;
                    break;
            }
            return query;
        }

        public override IQueryable<KumiAccount> GetQueryIncludes(IQueryable<KumiAccount> query)
        {
            return query.Include(r => r.AccountsTenancyProcessesAssoc)
                .Include(r => r.Claims)
                .Include(r => r.Charges);
        }

        public override IQueryable<KumiAccount> GetQueryOrder(IQueryable<KumiAccount> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField))
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdAccount);
                else
                    return query.OrderByDescending(p => p.IdAccount);
            }
            if (orderOptions.OrderField == "Account")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.Account);
                else
                    return query.OrderByDescending(p => p.Account);
            }
            return query;
        }
    }
}
