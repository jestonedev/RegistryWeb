using RegistryDb.Models;
using RegistryDb.Models.Entities.Common;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Enums;
using RegistryDb.Models.Entities.Claims;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Payments;
using RegistryServices.DataServices.KumiAccounts;

namespace RegistryServices.DataServices.Claims
{
    public class ClaimsAssignedAccountsDataService
    {
        private readonly RegistryContext registryContext;
        private readonly KumiAccountsTenanciesService kumiAccountsTenanciesService;

        public ClaimsAssignedAccountsDataService(RegistryContext registryContext,
            KumiAccountsTenanciesService kumiAccountsTenanciesService) {
            this.registryContext = registryContext;
            this.kumiAccountsTenanciesService = kumiAccountsTenanciesService;
        }

        public List<int> GetAccountIdsWithSameAddress(int idAccount)
        {
            var paymentAddressInfix = GetPaymentAccountAddressInfix(idAccount);

            var filteredObjects = paymentAddressInfix != null ? new List<PaymentAddressInfix> { paymentAddressInfix } : new List<PaymentAddressInfix>();

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
            return (from filteredRow in filteredObjects
                    join allRow in allObjects
                    on filteredRow.Infix equals allRow.AddressCode
                    select allRow.IdAccount).ToList();
        }

        public PaymentAddressInfix GetPaymentAccountAddressInfix(int idAccount)
        {
            return (from row in
                            (from row in registryContext.PaymentAccountPremisesAssoc
                             where row.IdAccount == idAccount
                             select new PaymentAddressInfix
                             {
                                 IdAccount = row.IdAccount,
                                 Infix = string.Concat("p", row.IdPremise)
                             }).Union(from row in registryContext.PaymentAccountSubPremisesAssoc
                                      where row.IdAccount == idAccount
                                      select new PaymentAddressInfix
                                      {
                                          IdAccount = row.IdAccount,
                                          Infix = string.Concat("sp", row.IdSubPremise)
                                      })
                    orderby row.Infix
                    group row.Infix by row.IdAccount into gs
                    select new PaymentAddressInfix
                    {
                        IdAccount = gs.Key,
                        Infix = string.Join("", gs)
                    }).FirstOrDefault();
        }

        public string GetAccountAddress(int idAccount)
        {
            var infixes = registryContext.GetAddressByAccountIds(new List<int> { idAccount }).Select(r => r.Address).ToList();
            return infixes.FirstOrDefault() ?? "";
        }

        public IList<AccountBase> GetAccounts(string text, string type, bool excludeAnnual)
        {
            if (type == "BKS")
                return registryContext.PaymentAccounts.Where(pa => pa.Account.Contains(text)).Take(100).Select(r => (AccountBase)r).ToList();
            else
            if (type == "KUMI")
                return registryContext.KumiAccounts.Where(pa => pa.Account.Contains(text) && (!excludeAnnual || pa.IdState != 2))
                    .Take(100).Select(r => (AccountBase)r).ToList();
            return null;
        }


        public Dictionary<int, List<Address>> GetRentObjectsBks(IEnumerable<int> idAccounts)
        {
            var premises = from paRow in registryContext.PaymentAccountPremisesAssoc
                           join premiseRow in registryContext.Premises.Include(r => r.IdStateNavigation)
                           on paRow.IdPremise equals premiseRow.IdPremises
                           join buildingRow in registryContext.Buildings
                           on premiseRow.IdBuilding equals buildingRow.IdBuilding
                           join streetRow in registryContext.KladrStreets
                           on buildingRow.IdStreet equals streetRow.IdStreet
                           join premiseTypesRow in registryContext.PremisesTypes
                           on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                           where idAccounts.Contains(paRow.IdAccount)
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
                              where idAccounts.Contains(paRow.IdAccount)
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

        public Dictionary<int, List<Address>> GetRentObjectsBks(IEnumerable<Claim> claims)
        {
            var ids = claims.Where(r => r.IdAccount != null).Select(r => r.IdAccount.Value).Distinct();
            return GetRentObjectsBks(ids);
        }

        public Dictionary<int, List<KumiAccountTenancyInfoVM>> GetTenancyInfoKumi(IEnumerable<Claim> claims)
        {
            var accounts = claims.Where(r => r.IdAccountKumi != null).Select(r => r.IdAccountKumiNavigation).Distinct();
            return kumiAccountsTenanciesService.GetTenancyInfo(accounts);
        }

        public Dictionary<int, List<KumiAccountTenancyInfoVM>> GetTenancyInfoKumi(IEnumerable<int> idAccounts)
        {
            var accounts = idAccounts.Select(r => new KumiAccount
            {
                IdAccount = r
            }).Distinct();
            return kumiAccountsTenanciesService.GetTenancyInfo(accounts);
        }

        public Dictionary<int, Payment> GetLastPaymentsInfo(IEnumerable<Claim> claims)
        {
            var ids = claims.Where(r => r.IdAccount != null).Select(r => r.IdAccount.Value).Union(
                claims.Where(r => r.IdAccountAdditional != null).Select(r => r.IdAccountAdditional.Value)
                ).Distinct();
            return GetLastPaymentsInfo(ids);
        }

        public Dictionary<int, Payment> GetLastPaymentsInfo(IEnumerable<int> idsAccount)
        {
            var maxDatePayments = from row in registryContext.Payments
                                  where idsAccount.Contains(row.IdAccount)
                                  group row.Date by row.IdAccount into gs
                                  select new
                                  {
                                      IdAccount = gs.Key,
                                      Date = gs.Max()
                                  };

            var lastPayments = from row in registryContext.Payments
                               join maxDatePaymentsRow in maxDatePayments
                               on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                               select row;

            var result =
                lastPayments.GroupBy(r => r.IdAccount)
                .Select(r => new { IdAccount = r.Key, Payment = r.FirstOrDefault() })
                .Where(r => r.Payment != null)
                .ToDictionary(v => v.IdAccount, v => v.Payment);
            return result;
        }

        public string GetAccountForCLaim(int idClaim)
        {
            var claim = registryContext.Claims.FirstOrDefault(c => c.IdClaim == idClaim);

            return claim.IdAccount != null ?
                    (from cl in registryContext.Claims
                     join pa in registryContext.PaymentAccounts
                     on cl.IdAccount equals pa.IdAccount
                     where cl.IdClaim == idClaim
                     select pa.Account).FirstOrDefault()
                : (from cl in registryContext.Claims
                   join ka in registryContext.KumiAccounts
                   on cl.IdAccountKumi equals ka.IdAccount
                   where cl.IdClaim == idClaim
                   select ka.Account).FirstOrDefault();
        }



        public PaymentAccount GetAccountBks(int idAccount)
        {
            return registryContext.PaymentAccounts
                .FirstOrDefault(pa => pa.IdAccount == idAccount);
        }

        public KumiAccount GetAccountKumi(int idAccount)
        {
            return registryContext.KumiAccounts
                .FirstOrDefault(a => a.IdAccount == idAccount);
        }
    }
}
