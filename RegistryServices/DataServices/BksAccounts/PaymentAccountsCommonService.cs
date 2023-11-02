using RegistryDb.Models;
using RegistryDb.Models.Entities.Payments;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RegistryServices.DataServices.BksAccounts
{
    public class PaymentAccountsCommonService
    {
        private readonly RegistryContext registryContext;

        public PaymentAccountsCommonService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        internal IQueryable<Payment> GetQuery()
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

        public IQueryable<Payment> GetPaymentsForMassReports(List<int> ids)
        {
            return GetQuery().Where(p => ids.Contains(p.IdAccount)).Include(p => p.PaymentAccountNavigation).AsNoTracking();
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
    }
}
