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
using RegistryServices.DataServices.Tenancies;

namespace RegistryServices.DataServices.KumiAccounts
{
    public class KumiAccountsTenanciesService
    {
        private readonly RegistryContext registryContext;
        private readonly TenancyPaymentsDataService tenancyPayments;

        public KumiAccountsTenanciesService(RegistryContext registryContext, TenancyPaymentsDataService tenancyPayments)
        {
            this.registryContext = registryContext;
            this.tenancyPayments = tenancyPayments;
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

            var objects = tenancyPayments.GetTenancyPaymentRentObjectInfo(accounts, loadPaymentHistory);

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
