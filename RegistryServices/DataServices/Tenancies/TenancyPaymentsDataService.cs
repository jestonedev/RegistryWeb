using Microsoft.Extensions.Caching.Distributed;
using RegistryDb.Models;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryDb.Models.Entities.Tenancies;
using RegistryDb.Models.SqlViews;
using RegistryWeb.Enums;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Threading.Tasks;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryServices.DataServices.Tenancies
{
    public class TenancyPaymentsDataService
    {
        private readonly RegistryContext registryContext;
        private readonly IDistributedCache cache;

        private static class CacheKeys {
            public static readonly string Payments18272014 = "TenancyPayments18272014";
            public static readonly string Payments15172022 = "TenancyPayments15172022";
            public static readonly string LastUpdate18272014 = "TenancyPaymentsHistoryLastUpdate18272014";
            public static readonly string LastUpdate15172022 = "TenancyPaymentsHistoryLastUpdate15172022";
        }

        public TenancyPaymentsDataService(RegistryContext registryContext, IDistributedCache cache)
        {
            this.registryContext = registryContext;
            this.cache = cache;
        }

        private IQueryable<TenancyPayment> TenancyPayments18272014
        {
            get
            {
                var cacheLastUpdate = GetTenancyPaymentHistoryLastUpdateFromCache(CacheKeys.LastUpdate18272014);
                var dbLastUpdateDate = registryContext.TenancyPaymentsHistoryLastUpdate.FirstOrDefault()?.LastUpdateDate;
                if (cache == null)
                {
                    return registryContext._TenancyPayments;
                } else
                if (cacheLastUpdate == null || cacheLastUpdate < dbLastUpdateDate)
                {
                    return GetTenancyPayments18272014WithCaching(dbLastUpdateDate).GetAwaiter().GetResult();
                } else
                {
                    var serialized = cache.GetString(CacheKeys.Payments18272014);
                    if (string.IsNullOrEmpty(serialized)) return GetTenancyPayments18272014WithCaching(dbLastUpdateDate).GetAwaiter().GetResult();
                    return JsonSerializer.Deserialize<List<TenancyPayment>>(serialized).AsQueryable();
                }
            }
        }

        private IQueryable<TenancyPaymentAfter28082019> TenancyPayments15172022
        {
            get
            {
                var cacheLastUpdate = GetTenancyPaymentHistoryLastUpdateFromCache(CacheKeys.LastUpdate15172022);
                var dbLastUpdateDate = registryContext.TenancyPaymentsHistoryLastUpdate.FirstOrDefault()?.LastUpdateDate;
                if (cache == null)
                {
                    return registryContext._TenancyPaymentsAfter28082019;
                }
                else
                if (cacheLastUpdate == null || cacheLastUpdate < dbLastUpdateDate)
                {
                    return GetTenancyPayments15172022WithCaching(dbLastUpdateDate).GetAwaiter().GetResult();
                }
                else
                {
                    var serialized = cache.GetString(CacheKeys.Payments15172022);
                    if (string.IsNullOrEmpty(serialized)) return GetTenancyPayments15172022WithCaching(dbLastUpdateDate).GetAwaiter().GetResult();
                    return JsonSerializer.Deserialize<List<TenancyPaymentAfter28082019>>(serialized).AsQueryable();
                }
            }
        }

        private DateTime? GetTenancyPaymentHistoryLastUpdateFromCache(string dateCacheKey)
        {
            if (cache == null) return null;
            var serialized = cache.GetString(dateCacheKey);
            if (string.IsNullOrEmpty(serialized)) return null;
            return JsonSerializer.Deserialize<DateTime>(serialized);
        }

        private async Task<IQueryable<TenancyPayment>> GetTenancyPayments18272014WithCaching(DateTime? updatedDate)
        {
            return await GetTenancyPaymentsWithCaching(updatedDate, CacheKeys.LastUpdate18272014, CacheKeys.Payments18272014,
                () => registryContext._TenancyPayments);
        }

        private async Task<IQueryable<TenancyPaymentAfter28082019>> GetTenancyPayments15172022WithCaching(DateTime? updatedDate)
        {
            return await GetTenancyPaymentsWithCaching(updatedDate, CacheKeys.LastUpdate15172022, CacheKeys.Payments15172022,
                () => registryContext._TenancyPaymentsAfter28082019);
        }

        private async Task<IQueryable<T>> GetTenancyPaymentsWithCaching<T>(DateTime? updatedDate,
            string dateCacheKey, string paymentsCacheKey,
            Func<IQueryable<T>> getPaymentsFunc)
        {
            var payments = getPaymentsFunc().ToList();
            await cache.SetStringAsync(dateCacheKey, JsonSerializer.Serialize(updatedDate ?? DateTime.Now));
            await cache.SetStringAsync(paymentsCacheKey, JsonSerializer.Serialize(payments));
            return payments.AsQueryable();
        }

        internal IEnumerable<TenancyPaymentCoeffitients> GetSubPremisesCoeffitionts15172022(IEnumerable<TenancySubPremiseAssoc> tspa, IEnumerable<TenancyProcess> tp)
        {
            var preparedData = (from tspaRow in tspa
                                join paymentRow in TenancyPayments15172022
                                on tspaRow.IdSubPremise equals paymentRow.IdSubPremises
                                select new TenancyPaymentCoeffitients
                                {
                                    IdProcess = tspaRow.IdProcess,
                                    IdObject = paymentRow.IdSubPremises ?? 0,
                                    Hb = paymentRow.Hb,
                                    K1 = paymentRow.K1,
                                    K2 = paymentRow.K2,
                                    K3 = paymentRow.K3,
                                    KC = paymentRow.KC,
                                    RentArea = tspaRow.RentTotalArea == null ? paymentRow.RentArea : tspaRow.RentTotalArea ?? 0
                                }).Distinct();

            return GetCoeffitionsForActiveAccountsOnly15172022(preparedData, tp);
        }

        internal IEnumerable<TenancyPaymentCoeffitients> GetPremisesCoeffitients15172022(IEnumerable<TenancyPremiseAssoc> tpa, IEnumerable<TenancyProcess> tp)
        {
            var preparedData = (from tpaRow in tpa
                               join paymentRow in TenancyPayments15172022
                                on tpaRow.IdPremise equals paymentRow.IdPremises
                               where paymentRow.IdSubPremises == null
                               select new TenancyPaymentCoeffitients
                               {
                                   IdProcess = tpaRow.IdProcess,
                                   IdObject = paymentRow.IdPremises ?? 0,
                                   Hb = paymentRow.Hb,
                                   K1 = paymentRow.K1,
                                   K2 = paymentRow.K2,
                                   K3 = paymentRow.K3,
                                   KC = paymentRow.KC,
                                   RentArea = tpaRow.RentTotalArea == null ? paymentRow.RentArea : tpaRow.RentTotalArea ?? 0
                               }).Distinct();

            return GetCoeffitionsForActiveAccountsOnly15172022(preparedData, tp);
        }

        internal IEnumerable<TenancyPaymentCoeffitients> GetBuildingsCoeffitients15172022(IEnumerable<TenancyBuildingAssoc> tpa, IEnumerable<TenancyProcess> tp)
        {
            var preparedData = (from tbaRow in tpa
                               join paymentRow in TenancyPayments15172022
                               on tbaRow.IdBuilding equals paymentRow.IdBuilding
                               where paymentRow.IdPremises == null
                               select new TenancyPaymentCoeffitients
                               {
                                   IdProcess = tbaRow.IdProcess,
                                   IdObject = paymentRow.IdBuilding,
                                   Hb = paymentRow.Hb,
                                   K1 = paymentRow.K1,
                                   K2 = paymentRow.K2,
                                   K3 = paymentRow.K3,
                                   KC = paymentRow.KC,
                                   RentArea = tbaRow.RentTotalArea == null ? paymentRow.RentArea : tbaRow.RentTotalArea ?? 0
                               }).Distinct();

            return GetCoeffitionsForActiveAccountsOnly15172022(preparedData, tp);
        }

        private IEnumerable<TenancyPaymentCoeffitients> GetCoeffitionsForActiveAccountsOnly15172022(IEnumerable<TenancyPaymentCoeffitients> preparedData,
            IEnumerable<TenancyProcess> tp)
        {
            return (from paymentRow in preparedData
                    join tpRow in tp on paymentRow.IdProcess equals tpRow.IdProcess
                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                      tpRow.TenancyPersons.Any()
                    select paymentRow).Distinct();
        }

        internal IEnumerable<TenancyPayment> GetPayments18272014(IEnumerable<TenancyProcess> tenancyProcesses)
        {
            var ids = tenancyProcesses.Select(r => r.IdProcess);

            var payments = from paymentsRow in TenancyPayments18272014
                            where ids.Contains(paymentsRow.IdProcess)
                            select paymentsRow;

            return from paymentRow in payments
                        join tpRow in tenancyProcesses
                        on paymentRow.IdProcess equals tpRow.IdProcess
                        where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                            tpRow.TenancyPersons.Any()
                        select paymentRow;
        }

        internal IEnumerable<TenancyPayment> GetPayments18272014(IEnumerable<Premise> premises)
        {
            var ids = premises.Select(p => p.IdPremises).ToList();
            return from paymentRow in TenancyPayments18272014
                where paymentRow.IdPremises != null && ids.Contains(paymentRow.IdPremises.Value)
                select paymentRow;
        }

        internal IEnumerable<TenancyPaymentRentObjectInfo> GetTenancyPaymentRentObjectInfo(
            IEnumerable<TenancyProcess> tenancyProcesses, IEnumerable<TenancyBuildingAssoc> tenancyBuildingAssocs, 
            IEnumerable<TenancyPremiseAssoc> tenancyPremiseAssocs, IEnumerable<TenancySubPremiseAssoc> tenancySubPremiseAssocs, bool loadPaymentHistory)
        {
            var payments18272014 = GetPayments18272014(tenancyProcesses.AsQueryable()).ToList();
            var tenancyIds = tenancyProcesses.Select(r => r.IdProcess).ToList();

            var tenancyBuildingsAssoc = (from tbaRow in registryContext.TenancyBuildingsAssoc
                                         where tenancyIds.Contains(tbaRow.IdProcess)
                                         select tbaRow).ToList();

            var payments15172022Buildings = GetBuildingsCoeffitients15172022(tenancyBuildingsAssoc.AsQueryable(), tenancyProcesses.AsQueryable()).ToList();

            var tenancyPremisesAssoc = (from tpaRow in registryContext.TenancyPremisesAssoc
                                        where tenancyIds.Contains(tpaRow.IdProcess)
                                        select tpaRow).ToList();

            var payments15172022Premises = GetPremisesCoeffitients15172022(tenancyPremisesAssoc.AsQueryable(), tenancyProcesses.AsQueryable()).ToList();

            var tenancySubPremisesAssoc = (from tspaRow in registryContext.TenancySubPremisesAssoc
                                           where tenancyIds.Contains(tspaRow.IdProcess)
                                           select tspaRow).ToList();

            var payments15172022SubPremises = GetSubPremisesCoeffitionts15172022(tenancySubPremisesAssoc.AsQueryable(), tenancyProcesses.AsQueryable()).ToList();

            var objects = GetTenancyPaymentRentObjectInfo(tenancyProcesses);

            var paymentHistoryBuildings = new List<TenancyPaymentHistory>();
            var paymentHistoryPremises = new List<TenancyPaymentHistory>();
            var paymentHistorySubPremises = new List<TenancyPaymentHistory>();
            if (loadPaymentHistory)
            {
                var buildingIds = objects.Where(r => r.RentObject.Address.AddressType == AddressTypes.Building)
                    .Select(r => int.Parse(r.RentObject.Address.Id)).ToList();
                paymentHistoryBuildings = (from row in registryContext.TenancyPaymentsHistory
                                           where buildingIds.Contains(row.IdBuilding) && row.IdPremises == null
                                           select row).ToList();

                var premisesIds = objects.Where(r => r.RentObject.Address.AddressType == AddressTypes.Premise)
                    .Select(r => int.Parse(r.RentObject.Address.Id)).ToList();
                paymentHistoryPremises = (from row in registryContext.TenancyPaymentsHistory
                                          where row.IdPremises != null && premisesIds.Contains(row.IdPremises.Value) && row.IdSubPremises == null
                                          select row).ToList();

                var subPremisesIds = objects.Where(r => r.RentObject.Address.AddressType == AddressTypes.SubPremise)
                    .Select(r => int.Parse(r.RentObject.Address.Id)).ToList();
                paymentHistorySubPremises = (from row in registryContext.TenancyPaymentsHistory
                                             where row.IdSubPremises != null && subPremisesIds.Contains(row.IdSubPremises.Value)
                                             select row).ToList();
            }

            foreach (var obj in objects)
            {
                if (obj.RentObject.Address.AddressType == AddressTypes.Building)
                {
                    obj.RentObject.Payment =
                        payments18272014.Where(r => r.IdProcess == obj.IdProcess && r.IdBuilding.ToString() == obj.RentObject.Address.Id && r.IdPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                       Math.Round(payments15172022Buildings.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdObject.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => r.Payment), 2);
                    obj.RentObject.TenancyPaymentHistory = paymentHistoryBuildings.Where(r => r.IdBuilding.ToString() == obj.RentObject.Address.Id).ToList();
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.Premise)
                {
                    obj.RentObject.Payment =
                        payments18272014.Where(r => r.IdProcess == obj.IdProcess && r.IdPremises.ToString() == obj.RentObject.Address.Id && r.IdSubPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(payments15172022Premises.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdObject.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => r.Payment), 2);
                    obj.RentObject.TenancyPaymentHistory = paymentHistoryPremises.Where(r => r.IdPremises.ToString() == obj.RentObject.Address.Id).ToList();
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.SubPremise)
                {
                    obj.RentObject.Payment =
                        payments18272014.Where(r => r.IdProcess == obj.IdProcess && r.IdSubPremises.ToString() == obj.RentObject.Address.Id).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(payments15172022SubPremises.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdObject.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => r.Payment), 2);
                    obj.RentObject.TenancyPaymentHistory = paymentHistorySubPremises.Where(r => r.IdSubPremises.ToString() == obj.RentObject.Address.Id).ToList();
                }
            }

            return objects;
        }

        private IEnumerable<TenancyPaymentRentObjectInfo> GetTenancyPaymentRentObjectInfo(IEnumerable<TenancyProcess> tenancyProcesses)
        {
            var tenancyIds = tenancyProcesses.Select(r => r.IdProcess).ToList();
            var buildings = (from tbaRow in registryContext.TenancyBuildingsAssoc
                             join buildingRow in registryContext.Buildings.Include(r => r.IdStateNavigation)
                             on tbaRow.IdBuilding equals buildingRow.IdBuilding
                             join streetRow in registryContext.KladrStreets
                             on buildingRow.IdStreet equals streetRow.IdStreet
                             where tenancyIds.Contains(tbaRow.IdProcess)
                             select new TenancyPaymentRentObjectInfo
                             {
                                 IdProcess = tbaRow.IdProcess,
                                 RentObject = new TenancyRentObject
                                 {
                                     Address = new Address
                                     {
                                         AddressType = AddressTypes.Building,
                                         Id = buildingRow.IdBuilding.ToString(),
                                         ObjectState = buildingRow.IdStateNavigation,
                                         IdParents = new Dictionary<string, string> {
                                            { AddressTypes.Street.ToString(), buildingRow.IdStreet }
                                        },
                                         Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House)
                                     },
                                     TotalArea = buildingRow.TotalArea,
                                     LivingArea = buildingRow.LivingArea,
                                     RentArea = tbaRow.RentTotalArea
                                 }
                             }).ToList();
            var premises = (from tpaRow in registryContext.TenancyPremisesAssoc
                            join premiseRow in registryContext.Premises.Include(r => r.IdStateNavigation)
                            on tpaRow.IdPremise equals premiseRow.IdPremises
                            join buildingRow in registryContext.Buildings
                            on premiseRow.IdBuilding equals buildingRow.IdBuilding
                            join streetRow in registryContext.KladrStreets
                            on buildingRow.IdStreet equals streetRow.IdStreet
                            join premiseTypesRow in registryContext.PremisesTypes
                            on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                            where tenancyIds.Contains(tpaRow.IdProcess)
                            select new TenancyPaymentRentObjectInfo
                            {
                                IdProcess = tpaRow.IdProcess,
                                RentObject = new TenancyRentObject
                                {
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
                                    },
                                    TotalArea = premiseRow.TotalArea,
                                    LivingArea = premiseRow.LivingArea,
                                    RentArea = tpaRow.RentTotalArea
                                }
                            }).ToList();
            var subPremises = (from tspaRow in registryContext.TenancySubPremisesAssoc
                               join subPremiseRow in registryContext.SubPremises.Include(r => r.IdStateNavigation)
                               on tspaRow.IdSubPremise equals subPremiseRow.IdSubPremises
                               join premiseRow in registryContext.Premises
                               on subPremiseRow.IdPremises equals premiseRow.IdPremises
                               join buildingRow in registryContext.Buildings
                               on premiseRow.IdBuilding equals buildingRow.IdBuilding
                               join streetRow in registryContext.KladrStreets
                               on buildingRow.IdStreet equals streetRow.IdStreet
                               join premiseTypesRow in registryContext.PremisesTypes
                               on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                               where tenancyIds.Contains(tspaRow.IdProcess)
                               select new TenancyPaymentRentObjectInfo
                               {
                                   IdProcess = tspaRow.IdProcess,
                                   RentObject = new TenancyRentObject
                                   {
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
                                       },
                                       TotalArea = subPremiseRow.TotalArea,
                                       LivingArea = subPremiseRow.LivingArea,
                                       RentArea = tspaRow.RentTotalArea
                                   }
                               }).ToList();
            return buildings.Union(premises).Union(subPremises);
        }

        internal IEnumerable<TenancyPaymentRentObjectInfo> GetTenancyPaymentRentObjectInfo(IEnumerable<KumiAccount> accounts, bool loadPaymentHistory)
        {
            var accountIds = (from account in accounts
                              select
                                  account.IdAccount).ToList();
            var accountTenancyAssocs = (from assoc in registryContext.KumiAccountsTenancyProcessesAssocs
                                        where accountIds.Contains(assoc.IdAccount)
                                        select assoc).ToList();
            var tenancyIds = accountTenancyAssocs.Select(r => r.IdProcess).Distinct();

            var tenancyProcesses = registryContext.TenancyProcesses.Include(r => r.TenancyRentPeriods)
                .Include(r => r.TenancyPersons).Include(r => r.TenancyReasons)
                .Where(r => tenancyIds.Contains(r.IdProcess)).ToList();

            var tenancyBuildingsAssoc = (from tbaRow in registryContext.TenancyBuildingsAssoc
                                         where tenancyIds.Contains(tbaRow.IdProcess)
                                         select tbaRow).ToList();

            var tenancyPremisesAssoc = (from tpaRow in registryContext.TenancyPremisesAssoc
                                        where tenancyIds.Contains(tpaRow.IdProcess)
                                        select tpaRow).ToList();

            var tenancySubPremisesAssoc = (from tspaRow in registryContext.TenancySubPremisesAssoc
                                           where tenancyIds.Contains(tspaRow.IdProcess)
                                           select tspaRow).ToList();

            return GetTenancyPaymentRentObjectInfo(tenancyProcesses, tenancyBuildingsAssoc,
                tenancyPremisesAssoc, tenancySubPremisesAssoc, loadPaymentHistory).ToList();
        }

        internal IEnumerable<TenancyPaymentRentObjectInfo> GetTenancyPaymentRentObjectInfo(IEnumerable<TenancyProcess> tenancyProcesses, bool loadPaymentHistory)
        {
            var ids = tenancyProcesses.Select(r => r.IdProcess);

            var tbaFiltered = (from tbaRow in registryContext.TenancyBuildingsAssoc
                               where ids.Contains(tbaRow.IdProcess)
                               select tbaRow).ToList();
            var tpaFiltered = (from tpaRow in registryContext.TenancyPremisesAssoc
                               where ids.Contains(tpaRow.IdProcess)
                               select tpaRow).ToList();

            var tspaFiltered = (from tspaRow in registryContext.TenancySubPremisesAssoc
                                where ids.Contains(tspaRow.IdProcess)
                                select tspaRow).ToList();

             return GetTenancyPaymentRentObjectInfo(tenancyProcesses, tbaFiltered, tpaFiltered, tspaFiltered, loadPaymentHistory).ToList();
        }
    }
}
