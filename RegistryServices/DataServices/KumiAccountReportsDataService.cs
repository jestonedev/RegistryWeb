using InvoiceGenerator;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Payments;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.ViewModel.KumiAccounts;
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
        private readonly KumiAccountsDataService accountDataService;

        public KumiAccountReportsDataService(RegistryContext registryContext, TenancyProcessesDataService tenancyProcessesData,
            KumiAccountsDataService accountDataService)
        {
            this.registryContext = registryContext;
            this.tenancyProcessesData = tenancyProcessesData;
            this.accountDataService = accountDataService;
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

        public List<InvoiceGeneratorParam> GetInvoiceGeneratorParam(List<int> idAccounts, DateTime onDate, string textmessage)
        {
            var paymentDate = new DateTime(onDate.Year, onDate.Month, 1);
            paymentDate = paymentDate.AddMonths(1).AddDays(-1);
            var charges = (from cRow in registryContext.KumiCharges
                          where idAccounts.Contains(cRow.IdAccount) && cRow.EndDate == paymentDate
                          select cRow).ToList();
            var accounts = (from aRow in registryContext.KumiAccounts
                            where idAccounts.Contains(aRow.IdAccount)
                            select aRow).ToList();
            var bksAccounts = (from aRow in registryContext.PaymentAccounts
                               where idAccounts.Contains(aRow.IdAccount)
                               select aRow).ToList();

            var addresses = registryContext.GetAddressByAccountIds(idAccounts);
            var tenants = registryContext.GetTenantsByAccountIds(idAccounts);
          

            var result = new List<InvoiceGeneratorParam>();
            foreach(var idAccount in idAccounts)
            {
                var charge = charges.FirstOrDefault(r => r.IdAccount == idAccount);
                var account = accounts.FirstOrDefault(r => r.IdAccount == idAccount);
                var bksAccount = bksAccounts.FirstOrDefault(r => r.IdAccount == idAccount);
                var addressList = addresses.Where(r => r.IdAccount == idAccount).ToList() ?? new List<KumiAccountAddressInfix>();
                var address = JoinAddresses(addressList.Select(r => r.Address).ToList());
                var tenant = tenants.FirstOrDefault(r => r.IdAccount == idAccount);
                var ob = new InvoiceGeneratorParam
                {
                    IdAccount = idAccount,
                    Address = address,
                    Account = account.Account,
                    Tenant = (string.IsNullOrEmpty(account.Owner) ? tenant?.Tenant : account.Owner) ?? "???",
                    OnDate = onDate,
                    BalanceInput = (charge?.InputTenancy + charge?.InputPenalty+ charge?.InputDgi + charge?.InputPkk + charge?.InputPadun).ToString().Replace(',', '.'),
                    ChargingTenancy = (charge?.ChargeTenancy + charge?.ChargeDgi + charge?.ChargePkk + charge?.ChargePadun).ToString().Replace(',', '.'),
                    ChargingPenalty = charge?.ChargePenalty.ToString().Replace(',', '.'),
                    Payed = (charge?.PaymentTenancy + charge?.PaymentPenalty + charge?.PaymentDgi + charge?.PaymentPkk + charge?.PaymentPadun).ToString().Replace(',', '.'),
                    RecalcTenancy = (charge?.RecalcTenancy + charge?.CorrectionTenancy +
                        charge?.RecalcDgi + charge?.CorrectionDgi + 
                        charge?.RecalcPkk + charge?.CorrectionPkk +
                        charge?.RecalcPadun + charge?.CorrectionPadun).ToString().Replace(',', '.'),
                    RecalcPenalty = (charge?.RecalcPenalty + charge?.CorrectionPenalty).ToString().Replace(',', '.'),
                    BalanceOutput = (charge?.OutputTenancy + charge?.OutputPenalty + charge?.OutputDgi + charge?.OutputPkk + charge?.OutputPadun)
                        .ToString().Replace(',', '.'),
                    TotalArea = addressList.Sum(r => r.TotalArea).ToString().Replace(',', '.'),
                    Prescribed = ((bksAccount != null && bksAccount.Prescribed != null && bksAccount.Prescribed != 0) ? bksAccount.Prescribed :  tenant?.Prescribed) ?? 0,
                    Emails = string.IsNullOrEmpty(tenant?.Emails) ? new List<string>() : tenant.Emails.Split(",").ToList(),
                    TextMessage = textmessage
                };

                result.Add(ob);
            }
            var cnt = result.Count;
            var tripleCount = (int)Math.Ceiling((decimal)cnt / 3);
            result = result.OrderBy(r => r.Address).ThenBy(r => r.Account).ToList();
            for (var i = 0; i < tripleCount; i++)
            {
                result[i].OrderGroup = i;
                if (tripleCount + i < result.Count)
                    result[tripleCount + i].OrderGroup = i;
                if (tripleCount * 2 + i < result.Count)
                    result[tripleCount * 2 + i].OrderGroup = i;
            }

            return result.OrderBy(r => r.OrderGroup).ToList();
        }

        private string JoinAddresses(List<string> addressList)
        {
            if (addressList.Count == 0) return "";
            if (addressList.Count == 1) return addressList.First();

            var address = "";
            var addressPartsList = addressList.Select(r => r.Split(", ", 5)).ToList();
            var groupedByRegion = addressPartsList.GroupBy(r => r[0]);
            foreach(var groupRegion in groupedByRegion)
            {
                if (groupRegion.All(r => r.Count() > 1))
                {
                    var groupedByStreet = groupRegion.GroupBy(r => r[1]);
                    
                    foreach (var groupStreet in groupedByStreet)
                    {
                        if (groupStreet.All(r => r.Count() > 2))
                        {
                            var groupedByHouse = groupStreet.GroupBy(r => r[2]);
                            foreach (var groupHouse in groupedByHouse)
                            {
                                if (groupHouse.All(r => r.Count() > 3))
                                {
                                    var groupedByPremise = groupHouse.GroupBy(r => r[3]);
                                    foreach(var groupPremise in groupedByPremise)
                                    {
                                        if (!string.IsNullOrWhiteSpace(address))
                                        {
                                            address += ", ";
                                        }
                                        address += groupRegion.Key + ", " + groupStreet.Key + ", " + groupHouse.Key + ", " + 
                                            groupPremise.Key + groupPremise.Aggregate("", (acc, v) => acc + (v.Length == 5 ? ", " + v[4] : ""));
                                    }
                                } else
                                {
                                    if (!string.IsNullOrWhiteSpace(address))
                                    {
                                        address += ", ";
                                    }
                                    address += groupRegion.Key + ", " + groupStreet.Key + ", " + groupHouse.Key;
                                }
                            }

                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                address += ", ";
                            }
                            address += groupRegion.Key + ", " + groupStreet.Key;
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        address += ", ";
                    }
                    address += groupRegion.Key;
                }
            }
            return address;
        }

        public LogInvoiceGenerator InvoiceGeneratorParamToLog(InvoiceGeneratorParam param, int errorCode)
        {
            return new LogInvoiceGenerator
            {
                IdAccount = param.IdAccount,
                AccountType = 2,
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

        public KumiAccount GetKumiAccount(int idAccount)
        {
            return accountDataService.GetKumiAccount(idAccount);
        }

        public List<KumiActChargeVM> GetActChargeVMs(int idAccount, DateTime atDate)
        {
            return accountDataService.GetActChargeVMs(idAccount, atDate);
        }
    }
}
