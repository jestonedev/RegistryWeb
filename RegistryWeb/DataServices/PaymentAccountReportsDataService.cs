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

                var ob = new InvoiceGeneratorParam
                {
                    IdAcconut = pay.IdAccount,
                    Address = pay.PaymentAccountNavigation.RawAddress,
                    Account = pay.PaymentAccountNavigation.Account,
                    Tenant = pay.Tenant,
                    OnData = onDate,
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
                    IdAcconut = idAccount,
                    Address = "",
                    Account = ac ?? "",
                    Tenant = null,
                    OnData = onDate,
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
