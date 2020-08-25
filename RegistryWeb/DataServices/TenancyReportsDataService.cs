using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.DataServices
{
    public class TenancyReportsDataService
    {
        private readonly RegistryContext registryContext;
        public TenancyReportsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        internal bool HasRentObjects(int idProcess)
        {
            return registryContext.TenancyBuildingsAssoc.FirstOrDefault(t => t.IdProcess == idProcess) != null ||
                registryContext.TenancyPremisesAssoc.FirstOrDefault(t => t.IdProcess == idProcess) != null ||
                registryContext.TenancySubPremisesAssoc.FirstOrDefault(t => t.IdProcess == idProcess) != null;
        }

        internal bool HasTenant(int idProcess)
        {
            return registryContext.TenancyPersons.FirstOrDefault(t => t.IdProcess == idProcess && t.IdKinship == 1 && t.ExcludeDate == null) != null;
        }

        internal bool HasRegistrationDateAndNum(int idProcess)
        {
            var tenancyProcess = registryContext.TenancyProcesses.FirstOrDefault(t => t.IdProcess == idProcess);
            return !string.IsNullOrEmpty(tenancyProcess.RegistrationNum) && tenancyProcess.RegistrationDate != null;
        }

        internal bool HasAgreement(int idProcess)
        {
            return registryContext.TenancyAgreements.FirstOrDefault(t => t.IdProcess == idProcess) != null; ;
        }

        internal int GetProcessIdForAgreement(int idAgreement)
        {
            return registryContext.TenancyAgreements.FirstOrDefault(t => t.IdAgreement == idAgreement)?.IdProcess ?? 0;
        }

        internal bool HasTenancies(int idProcess)
        {
            return registryContext.TenancyPersons.FirstOrDefault(t => t.IdProcess == idProcess) != null;
        }

        internal List<int> GetNoValidateContracts(List<int> ids)
        {
            return registryContext
                .TenancyProcesses.Where(tp => ids.Contains(tp.IdProcess) && string.IsNullOrEmpty(tp.RegistrationNum))
                .Select(tp => tp.IdProcess)
                .ToList();
        }

        internal void SetTenancyContractRegDate(List<int> ids, DateTime regDate)
        {
            foreach (var id in ids) {
                registryContext.TenancyProcesses
                    .FirstOrDefault(tp => tp.IdProcess == id)
                    .RegistrationDate = regDate;
            }
            registryContext.SaveChanges();
        }
    }
}
