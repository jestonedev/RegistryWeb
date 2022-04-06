using Microsoft.EntityFrameworkCore;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Payments;
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
        private readonly TenancyProcessesDataService tenancyProcessesData;
        public KumiAccountReportsDataService(RegistryContext registryContext, TenancyProcessesDataService tenancyProcessesData)
        {
            this.registryContext = registryContext;
            this.tenancyProcessesData = tenancyProcessesData;
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

        public InvoiceGeneratorParam GetInvoiceGeneratorParam(int idAccount, DateTime onDate, string textmessage)
        {
            var paymentDate = new DateTime(onDate.Year, onDate.Month, 1);
            paymentDate = paymentDate.AddMonths(1).AddDays(-1);
            var payKumi = registryContext.KumiCharges
                .Where(kc => kc.IdAccount == idAccount && kc.EndDate == paymentDate)
                .Include(c => c.Account).ThenInclude(c => c.KumiAccountAddressNavigation)
                    .Include(c => c.Account.TenancyProcesses)
                    .FirstOrDefault();
            if (payKumi != null)
            {
                int? idSubPremise = null;
                var idPremise = registryContext.TenancyPremisesAssoc
                    .Where(tpa => tpa.IdProcess == payKumi.Account.KumiAccountAddressNavigation.IdProcess)
                    .FirstOrDefault()
                    ?.IdPremise;
                if (idPremise == null)
                {
                    idSubPremise = registryContext.TenancySubPremisesAssoc
                        .Where(tspa => tspa.IdProcess == payKumi.Account.KumiAccountAddressNavigation.IdProcess)
                        .FirstOrDefault()
                        ?.IdSubPremise;
                }

                var processes = registryContext.TenancyActiveProcesses
                    .Where(tap => tap.IdPremises == idPremise && tap.IdSubPremises == idSubPremise);

                List<string> emails = new List<string>();
                foreach (var tp in processes)
                {
                    var curEmails = registryContext.TenancyPersons
                        .Where(per => per.IdProcess == tp.IdProcess && per.Email != null)
                        .Select(per => per.Email)
                        .ToList();
                    emails.AddRange(curEmails);
                }
                emails = new List<string>(emails.Distinct());

                var tenant = GetPersonPayment(idAccount);
                
                var totalArea = string.Join(";", tenancyProcessesData.GetRentObjects(payKumi.Account.TenancyProcesses
                  .Where(c => c.IdProcess == tenant.IdProcess))
                  .Select(t => t.Value.Select(c => c.TotalArea).ToList()).ToList()
                  .SelectMany(c => c).ToList());/**/
                var prescribed = tenancyProcessesData.GetTenancyProcess(tenant.IdProcess).TenancyPersons.Count;

                var ob = new InvoiceGeneratorParam
                {
                    IdAcconut = payKumi.IdAccount,
                    Address = payKumi.Account.KumiAccountAddressNavigation.Address,
                    Account = payKumi.Account.Account,
                    Tenant = string.Concat(tenant.Surname," ", tenant.Name, " ", tenant.Patronymic),
                    OnData = onDate,
                    BalanceInput = payKumi.InputTenancy.ToString().Replace(',', '.'),
                    Charging = (payKumi.ChargeTenancy+payKumi.RecalcTenancy).ToString().Replace(',', '.'),
                    Payed = (payKumi.PaymentTenancy+payKumi.PaymentPenalty).ToString().Replace(',', '.'),
                    Recalc = (payKumi.RecalcTenancy + payKumi.RecalcPenalty).ToString().Replace(',', '.'),
                    BalanceOutput = (payKumi.OutputTenancy + payKumi.OutputPenalty).ToString().Replace(',', '.'),
                    TotalArea = totalArea.ToString().Replace(',', '.'),
                    Prescribed = prescribed,
                    Emails = emails,
                    TextMessage = textmessage
                };
                return ob;
            }
            else
            {
                var ac = registryContext.KumiAccounts
                        .FirstOrDefault(a => a.IdAccount == idAccount).Account;
                return new InvoiceGeneratorParam
                {
                    IdAcconut = idAccount,
                    Address = "",
                    Account = ac ?? "",
                    Tenant = null,
                    OnData = onDate,
                    BalanceInput = "",
                    Charging = "",
                    Payed = "",
                    Recalc = "",
                    BalanceOutput = "",
                    TotalArea = "",
                    Prescribed = 0,
                    Emails = new List<string>(),
                    TextMessage = ""
                };
            }
        }

        public LogInvoiceGenerator InvoiceGeneratorParamToLog(InvoiceGeneratorParam param, int errorCode)
        {
            return new LogInvoiceGenerator
            {
                IdAccount = param.IdAcconut,
                CreateDate = DateTime.Now,
                OnDate = param.OnData,
                Emails = string.Join(", ", param.Emails).ToString(),
                ResultCode = errorCode
            };
        }

        public void AddLogInvoiceGenerator(LogInvoiceGenerator lig)
        {
            registryContext.LogInvoiceGenerator.Add(lig);
            registryContext.SaveChanges();
        }
    }
}
