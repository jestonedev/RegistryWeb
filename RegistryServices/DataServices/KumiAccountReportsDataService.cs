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
                    select  row).Include(c=> c.Account)
                    .Include(c=> c.Account.AccountsTenancyProcessesAssoc)
                    .FirstOrDefault();
        }
        public TenancyPerson  GetPersonPayment(List<TenancyProcess> processes)
        {
            return (from person in processes
                    .OrderBy(r => r.RegistrationDate ?? r.IssueDate)
                    .SelectMany(r => r.TenancyPersons)
                    where (person.IdKinship == 1) && (person.ExcludeDate == null)
                    select person).FirstOrDefault();
        }

        public Dictionary<string, object> GetInfoForReport(int idAccount)
        {
            var processes = registryContext.TenancyProcesses.Include(r => r.AccountsTenancyProcessesAssoc)
                .Include(r => r.TenancyPersons).Where(r => r.AccountsTenancyProcessesAssoc.Count(atpa => atpa.IdAccount == idAccount) > 0).ToList();
            var activeProcesses = processes
                .Where(r => r.TenancyPersons.Any(p => p.ExcludeDate == null || p.ExcludeDate > DateTime.Now)
                && (r.RegistrationNum == null || !r.RegistrationNum.EndsWith("н")));
            
            var rentObjects = tenancyProcessesData.GetRentObjects(activeProcesses);
            var totalArea = 0.0;
            var address = "";
            if (rentObjects != null && rentObjects.Any())
            {
                totalArea = rentObjects.SelectMany(r => r.Value).Sum(s => s.TotalArea);
                address = rentObjects.SelectMany(r => r.Value).Select(r => r.Address.Text)
                    .Aggregate((acc, v) => acc + "," + v);
            }
            var prescribed = activeProcesses
                .SelectMany(r => r.TenancyPersons.Where(p => p.ExcludeDate == null)).Count();

            List<string> emails = new List<string>();

            foreach (var tp in activeProcesses)
            {
                var curEmails = tp.TenancyPersons
                    .Where(per => per.IdProcess == tp.IdProcess && per.Email != null)
                    .Select(per => per.Email)
                    .ToList();
                emails.AddRange(curEmails);
            }
            emails = new List<string>(emails.Distinct());
            var tenant = GetPersonPayment(activeProcesses.ToList());

            var infoForReport = new Dictionary<string, object>()
            {
                ["totalArea"] = totalArea,
                ["address"] = address,
                ["prescribed"] = prescribed,
                ["emails"] = emails,
                ["tenant"] = string.Concat(tenant?.Surname, " ", tenant?.Name, " ", tenant?.Patronymic)

            };
            return infoForReport;

        }

        public InvoiceGeneratorParam GetInvoiceGeneratorParam(int idAccount, DateTime onDate, string textmessage)
        {
            var paymentDate = new DateTime(onDate.Year, onDate.Month, 1);
            paymentDate = paymentDate.AddMonths(1).AddDays(-1);
            var payKumi = registryContext.KumiCharges
                .Where(kc => kc.IdAccount == idAccount && kc.EndDate == paymentDate)
                .Include(c => c.Account)
                .FirstOrDefault();

            if (payKumi != null)
            {
                var infoForReport = GetInfoForReport(payKumi.IdAccount);

                var ob = new InvoiceGeneratorParam
                {
                    IdAcconut = payKumi.IdAccount,
                    Address = infoForReport.Where(c => c.Key.Contains("address")).Select(c => c.Value).FirstOrDefault().ToString(),
                    Account = payKumi.Account.Account,
                    Tenant = infoForReport.Where(c => c.Key.Contains("tenant")).Select(c => c.Value).FirstOrDefault().ToString(),
                    OnData = onDate,
                    BalanceInput = (payKumi.InputTenancy + payKumi.InputPenalty).ToString().Replace(',', '.'),
                    Charging = (payKumi.ChargeTenancy+payKumi.ChargePenalty).ToString().Replace(',', '.'),
                    Payed = (payKumi.PaymentTenancy+payKumi.PaymentPenalty).ToString().Replace(',', '.'),
                    Recalc = (payKumi.RecalcTenancy + payKumi.RecalcPenalty).ToString().Replace(',', '.'),
                    BalanceOutput = (payKumi.OutputTenancy + payKumi.OutputPenalty).ToString().Replace(',', '.'),
                    TotalArea = infoForReport.Where(c => c.Key.Contains("totalArea")).Select(c => c.Value).FirstOrDefault().ToString().Replace(',', '.'),
                    Prescribed = (int)infoForReport.Where(c => c.Key.Contains("prescribed")).Select(c => c.Value).FirstOrDefault(),
                    Emails = (List<string>)infoForReport.Where(c => c.Key.Contains("emails")).Select(c => c.Value).FirstOrDefault(),
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
                AccountType = 2,
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
