using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.DataServices
{
    public class TenancyReportsDataService
    {
        private readonly RegistryContext registryContext;
        public TenancyReportsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public bool HasRentObjects(int idProcess)
        {
            return registryContext.TenancyBuildingsAssoc.FirstOrDefault(t => t.IdProcess == idProcess) != null ||
                registryContext.TenancyPremisesAssoc.FirstOrDefault(t => t.IdProcess == idProcess) != null ||
                registryContext.TenancySubPremisesAssoc.FirstOrDefault(t => t.IdProcess == idProcess) != null;
        }

        public bool HasTenant(int idProcess)
        {
            return registryContext.TenancyPersons.FirstOrDefault(t => t.IdProcess == idProcess && t.IdKinship == 1 && t.ExcludeDate == null) != null;
        }

        public bool HasRegistrationDateAndNum(int idProcess)
        {
            var tenancyProcess = registryContext.TenancyProcesses.FirstOrDefault(t => t.IdProcess == idProcess);
            return !string.IsNullOrEmpty(tenancyProcess.RegistrationNum) && tenancyProcess.RegistrationDate != null;
        }

        public bool HasAgreement(int idProcess)
        {
            return registryContext.TenancyAgreements.FirstOrDefault(t => t.IdProcess == idProcess) != null; ;
        }

        public int GetProcessIdForAgreement(int idAgreement)
        {
            return registryContext.TenancyAgreements.FirstOrDefault(t => t.IdAgreement == idAgreement)?.IdProcess ?? 0;
        }

        public bool HasTenancies(int idProcess)
        {
            return registryContext.TenancyPersons.FirstOrDefault(t => t.IdProcess == idProcess) != null;
        }

        public List<int> GetNoValidateContracts(List<int> ids)
        {
            return registryContext
                .TenancyProcesses.Where(tp => ids.Contains(tp.IdProcess) && string.IsNullOrEmpty(tp.RegistrationNum))
                .Select(tp => tp.IdProcess)
                .ToList();
        }

        public void SetTenancyContractRegDate(List<int> ids, DateTime regDate)
        {
            foreach (var id in ids) {
                registryContext.TenancyProcesses
                    .FirstOrDefault(tp => tp.IdProcess == id)
                    .RegistrationDate = regDate;
            }
            registryContext.SaveChanges();
        }

        public void SetTenancyReason(List<int> ids, string reasonNumber, DateTime reasonDate, int idReasonType, bool isDeletePrevReasons)
        {
            foreach (var id in ids)
            {
                if (isDeletePrevReasons)
                {
                    var reasons = registryContext.TenancyReasons.Where(r => r.IdProcess == id);
                    foreach (var reason in reasons)
                    {
                        reason.Deleted = 1;
                    }
                }
                var reasonTemplate = registryContext.TenancyReasonTypes
                    .FirstOrDefault(rt => rt.IdReasonType == idReasonType)
                    ?.ReasonTemplate;
                var reasonPrepared = reasonTemplate.Replace("@reason_number@", reasonNumber).Replace("@reason_date@", reasonDate.ToString("dd.MM.yyyy"));
                var newReason = new TenancyReason
                {
                    IdProcess = id,
                    IdReasonType = idReasonType,
                    ReasonDate = reasonDate,
                    ReasonNumber = reasonNumber,
                    ReasonPrepared = reasonPrepared
                };
                registryContext.TenancyReasons.Add(newReason);
            }
            registryContext.SaveChanges();
        }
    }
}
