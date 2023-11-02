using RegistryDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryServices.DataServices.BksAccounts
{
    public class PaymentAccountsTenanciesService
    {
        private readonly RegistryContext registryContext;

        public PaymentAccountsTenanciesService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
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
                        (r.Surname + " " + r.Name + " " + r.Patronymic).Trim() == paymentTenant) > 0;
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

        public Dictionary<int, List<string>> GetTenantsEmailsModified(List<int> ids)
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
                        (r.Surname + " " + r.Name + " " + r.Patronymic).Trim() == paymentTenant) > 0;
                    if (!hasPerson)
                        continue;
                    var curEmails = registryContext.TenancyPersons
                        .Where(per => per.IdProcess == tp.IdProcess && per.Email != null)
                        .Select(per => new { per.PaymentAccount, per.Email })
                        .ToList();
                    foreach (var curE in curEmails)
                    {
                        if (curE.PaymentAccount == id)
                        {
                            emails.Add(curE.Email);
                        }
                    }
                }
                emails = emails.Distinct().ToList();
                emailsDic.Add(id, emails);
            }
            return emailsDic;
        }
    }
}
