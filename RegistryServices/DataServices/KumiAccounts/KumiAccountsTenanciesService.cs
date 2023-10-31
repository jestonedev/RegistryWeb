using RegistryDb.Models;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.ViewModel.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;
using RegistryWeb.Enums;
using RegistryDb.Models.Entities.Tenancies;

namespace RegistryServices.DataServices.KumiAccounts
{
    public class KumiAccountsTenanciesService
    {
        private readonly RegistryContext registryContext;

        public KumiAccountsTenanciesService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public Dictionary<int, List<KumiAccountTenancyInfoVM>> GetTenancyInfo(IEnumerable<KumiAccount> accounts, bool loadPaymentHistory = false)
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

            var buildings = (from tbaRow in registryContext.TenancyBuildingsAssoc
                             join buildingRow in registryContext.Buildings.Include(r => r.IdStateNavigation)
                             on tbaRow.IdBuilding equals buildingRow.IdBuilding
                             join streetRow in registryContext.KladrStreets
                             on buildingRow.IdStreet equals streetRow.IdStreet
                             where tenancyIds.Contains(tbaRow.IdProcess)
                             select new
                             {
                                 tbaRow.IdProcess,
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
                            select new
                            {
                                tpaRow.IdProcess,
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
                               select new
                               {
                                   tspaRow.IdProcess,
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

            var paymentHistoryBuildings = new List<TenancyPaymentHistory>();
            var paymentHistoryPremises = new List<TenancyPaymentHistory>();
            var paymentHistorySubPremises = new List<TenancyPaymentHistory>();
            if (loadPaymentHistory)
            {
                var buildingIds = buildings.Select(r => int.Parse(r.RentObject.Address.Id)).ToList();
                paymentHistoryBuildings = (from row in registryContext.TenancyPaymentsHistory
                                           where buildingIds.Contains(row.IdBuilding) && row.IdPremises == null
                                           select row).ToList();

                var premisesIds = premises.Select(r => int.Parse(r.RentObject.Address.Id)).ToList();
                paymentHistoryPremises = (from row in registryContext.TenancyPaymentsHistory
                                          where row.IdPremises != null && premisesIds.Contains(row.IdPremises.Value) && row.IdSubPremises == null
                                          select row).ToList();

                var subPremisesIds = subPremises.Select(r => int.Parse(r.RentObject.Address.Id)).ToList();
                paymentHistorySubPremises = (from row in registryContext.TenancyPaymentsHistory
                                             where row.IdSubPremises != null && subPremisesIds.Contains(row.IdSubPremises.Value)
                                             select row).ToList();
            }

            var objects = buildings.Union(premises).Union(subPremises).ToList();

            var payments = (from paymentsRow in registryContext.TenancyPayments
                            where tenancyIds.Contains(paymentsRow.IdProcess)
                            select paymentsRow).ToList();

            payments = (from paymentRow in payments
                        join tpRow in tenancyProcesses
                        on paymentRow.IdProcess equals tpRow.IdProcess
                        where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                            tpRow.TenancyPersons.Any()
                        select paymentRow).ToList();

            var prePaymentsAfter28082019Buildings = (from tbaRow in registryContext.TenancyBuildingsAssoc
                                                     join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                     on tbaRow.IdBuilding equals paymentRow.IdBuilding
                                                     where paymentRow.IdPremises == null && tenancyIds.Contains(tbaRow.IdProcess)
                                                     select new
                                                     {
                                                         tbaRow.IdProcess,
                                                         paymentRow.IdBuilding,
                                                         paymentRow.Hb,
                                                         paymentRow.K1,
                                                         paymentRow.K2,
                                                         paymentRow.K3,
                                                         paymentRow.KC,
                                                         RentArea = tbaRow.RentTotalArea == null ? paymentRow.RentArea : tbaRow.RentTotalArea
                                                     }).Distinct().ToList();

            var paymentsAfter28082019Buildings = (from paymentRow in prePaymentsAfter28082019Buildings
                                                  join tpRow in tenancyProcesses
                                                      on paymentRow.IdProcess equals tpRow.IdProcess
                                                  where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                    tpRow.TenancyPersons.Any()
                                                  select paymentRow).Distinct().ToList();

            var prePaymentsAfter28082019Premises = (from tpaRow in registryContext.TenancyPremisesAssoc
                                                    join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tpaRow.IdPremise equals paymentRow.IdPremises
                                                    where paymentRow.IdSubPremises == null && tenancyIds.Contains(tpaRow.IdProcess)
                                                    select new
                                                    {
                                                        tpaRow.IdProcess,
                                                        paymentRow.IdPremises,
                                                        paymentRow.Hb,
                                                        paymentRow.K1,
                                                        paymentRow.K2,
                                                        paymentRow.K3,
                                                        paymentRow.KC,
                                                        RentArea = tpaRow.RentTotalArea == null ? paymentRow.RentArea : tpaRow.RentTotalArea
                                                    }).Distinct().ToList();

            var paymentsAfter28082019Premises = (from paymentRow in prePaymentsAfter28082019Premises
                                                 join tpRow in tenancyProcesses
                                                     on paymentRow.IdProcess equals tpRow.IdProcess
                                                 where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                   tpRow.TenancyPersons.Any()
                                                 select paymentRow).Distinct().ToList();

            var prePaymentsAfter28082019SubPremises = (from tspaRow in registryContext.TenancySubPremisesAssoc
                                                       join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tspaRow.IdSubPremise equals paymentRow.IdSubPremises
                                                       where tenancyIds.Contains(tspaRow.IdProcess)
                                                       select new
                                                       {
                                                           tspaRow.IdProcess,
                                                           paymentRow.IdSubPremises,
                                                           paymentRow.Hb,
                                                           paymentRow.K1,
                                                           paymentRow.K2,
                                                           paymentRow.K3,
                                                           paymentRow.KC,
                                                           RentArea = tspaRow.RentTotalArea == null ? paymentRow.RentArea : tspaRow.RentTotalArea
                                                       }).Distinct().ToList();



            var paymentsAfter28082019SubPremises = (from paymentRow in prePaymentsAfter28082019SubPremises
                                                    join tpRow in tenancyProcesses
                                                        on paymentRow.IdProcess equals tpRow.IdProcess
                                                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                      tpRow.TenancyPersons.Any()
                                                    select paymentRow).Distinct().ToList();

            foreach (var obj in objects)
            {
                if (obj.RentObject.Address.AddressType == AddressTypes.Building)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdBuilding.ToString() == obj.RentObject.Address.Id && r.IdPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                       Math.Round(paymentsAfter28082019Buildings.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdBuilding.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                    obj.RentObject.TenancyPaymentHistory = paymentHistoryBuildings.Where(r => r.IdBuilding.ToString() == obj.RentObject.Address.Id).ToList();
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.Premise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdPremises.ToString() == obj.RentObject.Address.Id && r.IdSubPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019Premises.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                    obj.RentObject.TenancyPaymentHistory = paymentHistoryPremises.Where(r => r.IdPremises.ToString() == obj.RentObject.Address.Id).ToList();
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.SubPremise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdSubPremises.ToString() == obj.RentObject.Address.Id).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019SubPremises.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdSubPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                    obj.RentObject.TenancyPaymentHistory = paymentHistorySubPremises.Where(r => r.IdSubPremises.ToString() == obj.RentObject.Address.Id).ToList();
                }
            }

            var result = new Dictionary<int, List<KumiAccountTenancyInfoVM>>();
            foreach (var accountId in accountIds)
            {
                if (!result.ContainsKey(accountId))
                    result.Add(accountId, new List<KumiAccountTenancyInfoVM>());
                var currentAccountTenancyAssoc = accountTenancyAssocs.Where(r => r.IdAccount == accountId);
                foreach (var currentAssoc in currentAccountTenancyAssoc)
                {
                    var tenancyProcess = tenancyProcesses.FirstOrDefault(r => r.IdProcess == currentAssoc.IdProcess);
                    if (tenancyProcess == null) continue;
                    var rentObjects = objects.Where(r => r.IdProcess == tenancyProcess.IdProcess).Select(r => r.RentObject).ToList();
                    var tenancyInfo = new KumiAccountTenancyInfoVM
                    {
                        RentObjects = rentObjects,
                        TenancyProcess = tenancyProcess,
                        Tenant = tenancyProcess.TenancyPersons.FirstOrDefault(r => r.ExcludeDate == null && r.IdKinship == 1),
                        AccountAssoc = currentAssoc
                    };
                    result[accountId].Add(tenancyInfo);
                }
            }
            return result;
        }
    }
}
