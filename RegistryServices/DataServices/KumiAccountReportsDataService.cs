using Microsoft.EntityFrameworkCore;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.DataServices
{
    public class KumiAccountReportsDataService
    {
        private readonly RegistryContext registryContext;
        public KumiAccountReportsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }
        public KumiCharge GetLastPayment(int idAccount)
        {
            var maxDatePayments = from row in registryContext.KumiCharges
                                  group row.EndDate by row.IdAccount into gs
                                  select new
                                  {
                                      IdAccount = gs.Key,
                                      EndDate = gs.Max()
                                  };


            return (from row in registryContext.KumiCharges
                    join maxDatePaymentsRow in maxDatePayments
                    on new { row.IdAccount, row.EndDate }
                    equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.EndDate }
                    where row.IdAccount == idAccount
                    select  row).Include(c=> c.Account).ThenInclude(c=> c.KumiAccountAddressNavigation)
                    .Include(c=> c.Account.TenancyProcesses)
                    .FirstOrDefault();
        }
        public TenancyPerson  GetPersonPayment(int idAccount)
        {
           return (from person in registryContext.TenancyPersons
                    join address in registryContext.KumiAccountAddresses
                    on person.IdProcess equals address.IdProcess
                    where (person.IdKinship == 1) && (person.ExcludeDate == null) && (address.IdAccount == idAccount)
                    select person).FirstOrDefault();
        }
        
    }
}
