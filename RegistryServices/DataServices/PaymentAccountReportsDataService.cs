using InvoiceGenerator;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Payments;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.DataServices
{
    public class PaymentAccountReportsDataService
    {
        private readonly RegistryContext registryContext;
        public PaymentAccountReportsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public Payment GetLastPayment(int idAccount)
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
                    where row.IdAccount == idAccount
                    select row).Include(p => p.PaymentAccountNavigation).FirstOrDefault();
        }

        public InvoiceGeneratorParam GetInvoiceGeneratorParam(int idAccount, DateTime onDate, string textmessage)
        {
            var paymentDate = new DateTime(onDate.Year, onDate.Month, 1);
            paymentDate = paymentDate.AddMonths(1).AddDays(-1);
            var pay = registryContext.Payments
                .Where(p => p.IdAccount == idAccount && p.Date == paymentDate)
                .Include(p => p.PaymentAccountNavigation)
                .FirstOrDefault();
            if (pay != null)
            {
                int? idSubPremise = null;
                var idPremise = registryContext.PaymentAccountPremisesAssoc
                    .Where(papa => papa.IdAccount == pay.IdAccount)
                    .FirstOrDefault()
                    ?.IdPremise;
                if (idPremise == null)
                {
                    idSubPremise = registryContext.PaymentAccountSubPremisesAssoc
                        .Where(paspa => paspa.IdAccount == pay.IdAccount)
                        .FirstOrDefault()
                        ?.IdSubPremise;
                }

                var processes = registryContext.TenancyActiveProcesses
                    .Where(tap => tap.IdPremises == idPremise && tap.IdSubPremises == idSubPremise);

                var paymentTenant = registryContext.Payments.Where(r => r.IdAccount == idAccount).OrderByDescending(r => r.Date)
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
                        .Select(per => new { per.Email, per.PaymentAccount })
                        .ToList();
                    var emailsCount = 0;
                    foreach (var curE in curEmails)
                    {
                        if (curE.PaymentAccount == idAccount)
                        {
                            emails.Add(curE.Email);
                            emailsCount++;
                        }
                    }
                    if (emailsCount == 0) {
                        foreach (var curE in curEmails)
                        {
                            emails.Add(curE.Email);
                            emailsCount++;
                        }
                    }
                    // emails.AddRange(curEmails);
                }
                emails = new List<string>(emails.Distinct());

                var ob = new InvoiceGeneratorParam
                {
                    IdAccount = pay.IdAccount,
                    Address = pay.PaymentAccountNavigation.RawAddress,
                    Account = pay.PaymentAccountNavigation.Account,
                    Tenant = pay.Tenant,
                    OnDate = onDate,
                    BalanceInput = pay.BalanceInput.ToString().Replace(',', '.'),
                    ChargingTenancy = (pay.ChargingTenancy+pay.ChargingDgi).ToString().Replace(',', '.'),
                    ChargingPenalty = pay.ChargingPenalties.ToString().Replace(',', '.'),
                    Payed = (pay.PaymentTenancy+pay.PaymentPenalties+pay.PaymentDgi).ToString().Replace(',', '.'),
                    RecalcTenancy = (pay.RecalcTenancy+pay.RecalcDgi).ToString().Replace(',', '.'),
                    RecalcPenalty = pay.RecalcPenalties.ToString().Replace(',', '.'),
                    BalanceOutput = pay.BalanceOutputTotal.ToString().Replace(',', '.'),
                    TotalArea = pay.TotalArea.ToString().Replace(',', '.'),
                    Prescribed = pay.Prescribed,
                    Emails = emails,
                    TextMessage = textmessage
                };
                return ob;
            }
            else
            {
                var ac = registryContext.PaymentAccounts
                        .FirstOrDefault(a => a.IdAccount == idAccount).Account;
                return new InvoiceGeneratorParam
                {
                    IdAccount = idAccount,
                    Address = "",
                    Account = ac ?? "",
                    Tenant = null,
                    OnDate = onDate,
                    BalanceInput = "",
                    ChargingTenancy = "",
                    ChargingPenalty = "",
                    Payed = "",
                    RecalcTenancy = "",
                    RecalcPenalty = "",
                    BalanceOutput = "",
                    TotalArea = "",
                    Prescribed = 0,
                    Emails = new List<string>(),
                    TextMessage=""
                };
            }
        }

        public LogInvoiceGenerator InvoiceGeneratorParamToLog(InvoiceGeneratorParam param, int errorCode)
        {
            return new LogInvoiceGenerator
            {
                IdAccount = param.IdAccount,
                AccountType = 1,
                CreateDate = DateTime.Now,
                OnDate = param.OnDate,
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
