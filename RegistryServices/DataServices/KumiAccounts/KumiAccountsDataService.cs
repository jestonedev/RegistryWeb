using RegistryDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.SqlViews;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewOptions;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.Enums;
using RegistryServices.Models.KumiAccounts;
using RegistryWeb.DataHelpers;
using RegistryDb.Models.Entities.Payments;
using RegistryServices.DataFilterServices;
using RegistryWeb.DataServices;
using RegistryWeb.SecurityServices;

namespace RegistryServices.DataServices.KumiAccounts
{
    public class KumiAccountsDataService : ListDataService<KumiAccountsVM, KumiAccountsFilter>
    {
        private readonly KumiAccountsClaimsService claimsService;
        private readonly KumiAccountsTenanciesService tenanciesService;
        private readonly KumiAccountsCalculationService calculationService;
        private readonly SecurityService securityService;
        private readonly IFilterService<KumiAccount, KumiAccountsFilter> filterService;

        public KumiAccountsDataService(RegistryContext registryContext, AddressesDataService addressesDataService,
            KumiAccountsClaimsService claimsService,
            KumiAccountsTenanciesService tenanciesService,
            KumiAccountsCalculationService calculationService,
            SecurityService securityService,
            FilterServiceFactory<IFilterService<KumiAccount, KumiAccountsFilter>> filterServiceFactory
            ) : base(registryContext, addressesDataService)
        {
            this.claimsService = claimsService;
            this.tenanciesService = tenanciesService;
            this.calculationService = calculationService;
            this.securityService = securityService;
            filterService = filterServiceFactory.CreateInstance();
        }

        public override KumiAccountsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiAccountsFilter filterOptions)
        {
            var vm = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            vm.AccountStates = new SelectList(registryContext.KumiAccountStates.ToList(), "IdState", "State");
            return vm;
        }

        public bool AccountExists(string account)
        {
            return registryContext.KumiAccounts.Count(r => r.Account == account) != 0;
        }

        public string GetNextKumiAccountNumber()
        {
            return registryContext.GetNextKumiAccountNumber();
        }

        public string GetAccountAddress(int idAccount)
        {
            var addresses = registryContext.GetAddressByAccountIds(new List<int> { idAccount });
            if (addresses.Count == 0) return null;
            return AddressHelper.JoinAddresses(addresses.Select(r => r.Address).ToList());
        }

        public List<KumiAccount> GetKumiAccountsOnSameAddress(int idAccount)
        {
            var addresses = registryContext.GetAddressByAccountIds(new List<int> { idAccount });
            if (addresses.Count == 0) return new List<KumiAccount> { GetKumiAccount(idAccount) };
            var accountIds = registryContext.GetKumiAccountIdsByAddressInfixes(addresses.Select(r => r.Infix).ToList()).Distinct();
            var result = new List<KumiAccount>();
            foreach(var accountId in accountIds)
            {
                result.Add(GetKumiAccount(accountId));
            }
            return result;
        }
       
        public void SaveDescriptionForAddress(int idAccount, string description)
        {
            var addresses = registryContext.GetAddressByAccountIds(new List<int> { idAccount });
            var accountIds = new List<int>();
            if (addresses.Count == 0)
                accountIds.Add(idAccount);
            else
                accountIds.AddRange(registryContext.GetKumiAccountIdsByAddressInfixes(addresses.Select(r => r.Infix).ToList()));
            var accounts = registryContext.KumiAccounts.Where(r => accountIds.Contains(r.IdAccount));
            foreach(var account in accounts)
            {
                account.Description = description;
            }
            registryContext.SaveChanges();
        }

        public void AddChargeCorrection(int idAccount, decimal tenancyValue, decimal penaltyValue,
            decimal dgiValue, decimal pkkValue, decimal padunValue,
            decimal paymentTenancyValue, decimal paymentPenaltyValue, 
            decimal paymentDgiValue, decimal paymentPkkValue, decimal paymentPadunValue,
            DateTime atDate, string description, int? idAccountMirror)
        {
            var currentDay = DateTime.Now.Date;
            var periodStartDate = currentDay.AddDays(-currentDay.Day + 1);
            if (currentDay.Day >= 25)
            {
                periodStartDate = periodStartDate.AddMonths(1);
            }
            var dbCharge = registryContext.KumiCharges.FirstOrDefault(r => r.IdAccount == idAccount && r.StartDate == periodStartDate);
            if (dbCharge == null)
                registryContext.KumiCharges.Add(new KumiCharge {
                    IdAccount = idAccount,
                    StartDate = periodStartDate,
                    EndDate = periodStartDate.AddMonths(1).AddDays(-1),
                    IsBksCharge = 0,
                    Hidden = 0
                });

            registryContext.KumiChargeCorrections.Add(new KumiChargeCorrection {
                IdAccount = idAccount,
                TenancyValue = tenancyValue,
                PenaltyValue = penaltyValue,
                DgiValue = dgiValue,
                PkkValue = pkkValue,
                PadunValue = padunValue,
                PaymentTenancyValue = paymentTenancyValue,
                PaymentPenaltyValue = paymentPenaltyValue,
                PaymentDgiValue = paymentDgiValue,
                PaymentPkkValue = paymentPkkValue,
                PaymentPadunValue = paymentPadunValue,
                Date = atDate,
                Description = description
            });

            if(idAccountMirror.HasValue && idAccountMirror.Value!=idAccount)
                registryContext.KumiChargeCorrections.Add(new KumiChargeCorrection
                {
                    IdAccount = idAccountMirror.Value,
                    TenancyValue = 0,
                    PenaltyValue = 0,
                    DgiValue = 0,
                    PkkValue = 0,
                    PadunValue = 0,
                    PaymentTenancyValue = -paymentTenancyValue,
                    PaymentPenaltyValue = -paymentPenaltyValue,
                    PaymentDgiValue = -paymentDgiValue,
                    PaymentPkkValue = -paymentPkkValue,
                    PaymentPadunValue = -paymentPadunValue,
                    Date = atDate,
                    Description = description
                });

            registryContext.SaveChanges();
        }

        public int GetIdAccountByCorrection(int idCorrection)
        {
            return registryContext.KumiChargeCorrections.Where(r => r.IdCorrection == idCorrection).FirstOrDefault()?.IdAccount ?? 0;
        }

        public int[] SplitAccount(int idAccount, DateTime onDate, string description, List<SplitAccountModel> splitAccounts)
        {
            var resultAccounts = new List<KumiAccount>();
            var oldAccount = registryContext.KumiAccounts.Include(r => r.AccountsTenancyProcessesAssoc)
                .Where(r => r.IdAccount == idAccount).FirstOrDefault();
            if (oldAccount == null) throw new ApplicationException("Не удалось найти лицевой счет с идентификатором "+idAccount);
            foreach(var newAccount in splitAccounts)
            {
                var account = new KumiAccount
                {
                    CreateDate = DateTime.Now.Date,
                    Account = newAccount.Account,
                    AccountsTenancyProcessesAssoc = (oldAccount?.AccountsTenancyProcessesAssoc ?? new List<KumiAccountsTenancyProcessesAssoc>())
                        .Select(r => new KumiAccountsTenancyProcessesAssoc {
                            IdProcess = r.IdProcess,
                            Fraction = newAccount.Fraction
                        }).ToList(),
                    IdState = 1,
                    Owner = newAccount.Owner,
                    StartChargeDate = onDate,
                    Description = description
                };
                registryContext.KumiAccounts.Add(account);
                resultAccounts.Add(account);
            }
            oldAccount.StopChargeDate = onDate.AddDays(-1);
            if (!string.IsNullOrEmpty(description))
            {
                if (!string.IsNullOrEmpty(oldAccount.Description))
                {
                    oldAccount.Description += "\r\n";
                }
                oldAccount.Description += description;
            }
            registryContext.KumiAccounts.Update(oldAccount);
            registryContext.SaveChanges();

            var resultAccountIds = resultAccounts.Select(r => r.IdAccount).ToList();
            calculationService.RecalculateAccounts(resultAccountIds.Union(new List<int> { oldAccount.IdAccount }).ToList(), KumiAccountRecalcTypeEnum.AddRecalc, null, true);

            if (onDate <= DateTime.Now.Date)
            {
                oldAccount.IdState = 3;
                registryContext.KumiAccounts.Update(oldAccount);
                registryContext.SaveChanges();
            }

            return resultAccountIds.ToArray();
            // Проверить таблицы денормализации после создания ЛС (если они автоматом не обновились, то доработать код)
        }

        public void DeleteChargeCorrection(int idCorrection)
        {
            var correction = registryContext.KumiChargeCorrections.FirstOrDefault(r => r.IdCorrection == idCorrection);
            registryContext.KumiChargeCorrections.Remove(correction);
            registryContext.SaveChanges();
        }

        public KumiChargeCorrectionsVM GetAccountCorrectionsVm(int idAccount)
        {
            var viewModel = new KumiChargeCorrectionsVM();
            viewModel.PageOptions = new PageOptions();
            var corrections = registryContext.KumiChargeCorrections.Where(r => r.IdAccount == idAccount).OrderByDescending(r => r.Date);
            viewModel.PageOptions.TotalRows = corrections.Count();
            var count = corrections.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.ChargeCorrections = corrections.Skip((viewModel.PageOptions.CurrentPage - 1) * viewModel.PageOptions.SizePage)
                .Take(viewModel.PageOptions.SizePage);
            viewModel.Account = GetKumiAccount(idAccount);
            return viewModel;
        }

        public List<KumiAccount> GetAccountsForTenancies(List<TenancyProcess> tenancies)
        {
            var tenanciIds = tenancies.Select(r => r.IdProcess);
            var accounts = from row in registryContext.KumiAccounts.Include(r => r.State).Include(r => r.AccountsTenancyProcessesAssoc)
                           where row.AccountsTenancyProcessesAssoc.Count(r => tenanciIds.Contains(r.IdProcess)) > 0
                           select row;
            return accounts.ToList();
        }

        public string GetTenantByIdAccount(int idAccount)
        {
            var tenants = registryContext.GetTenantsByAccountIds(new List<int> { idAccount });
            return tenants.FirstOrDefault()?.Tenant;
        }

        public List<KumiActChargeVM> GetActChargeVMs(int idAccount, DateTime atDate)
        {
            var charges = registryContext.KumiCharges.Include(r => r.PaymentCharges).Where(r => r.IdAccount == idAccount && r.EndDate <= atDate).ToList();
            var chargeIds = charges.Select(r => r.IdCharge);

            var paymentIds = charges.SelectMany(r => r.PaymentCharges).Select(r => r.IdPayment);

            var account = registryContext.KumiAccounts.Include(r => r.Charges).FirstOrDefault(r => r.IdAccount == idAccount);
            if (account == null) return new List<KumiActChargeVM>();

            // Берем только платежи после даты последнего начисления БКС
            DateTime? maxBksCharegeDate = null;
            if (account.Charges.Any(r => r.IsBksCharge == 1))
            {
                maxBksCharegeDate = account.Charges.Where(r => r.IsBksCharge == 1).Select(r => r.EndDate).Max();
                maxBksCharegeDate = maxBksCharegeDate.Value.AddDays(-maxBksCharegeDate.Value.Day + 1).AddDays(24); // 25 число месяца, когда было начисление БКС
            }

            // Оригиналы платежей и ПИР для добавления в акт
            var payments = registryContext.KumiPayments.Include(r => r.PaymentCharges).Where(r => paymentIds.Contains(r.IdPayment)
                && (maxBksCharegeDate == null || (r.DateExecute ?? r.DateIn ?? r.DateDocument).Value > maxBksCharegeDate)).ToList();


            var bksPayments = new List<KumiPayment>();
            var idIndex = 0;
            foreach(var charge in registryContext.KumiCharges.Where(r => r.IdAccount == account.IdAccount && r.IsBksCharge == 1 && r.PaymentTenancy + r.PaymentPenalty > 0))
            {

                bksPayments.Add(new KumiPayment
                {
                    IdPayment = Int32.MaxValue - idIndex,
                    DateIn = charge.StartDate,
                    DateExecute = charge.StartDate,
                    DateDocument = charge.StartDate,
                    Sum = charge.PaymentTenancy + charge.PaymentPenalty,
                    PaymentCharges = new List<KumiPaymentCharge> {
                            new KumiPaymentCharge {
                                Date = charge.StartDate,
                                TenancyValue = charge.PaymentTenancy,
                                IdCharge = charge.IdCharge,
                                IdDisplayCharge = charge.IdCharge,
                            },
                            new KumiPaymentCharge {
                                Date = charge.StartDate,
                                PenaltyValue = charge.PaymentPenalty,
                                IdCharge = charge.IdCharge,
                                IdDisplayCharge = charge.IdCharge,
                            },
                        }
                });
                idIndex++;
            }

            payments.AddRange(bksPayments);

            var resultPayments = payments.Select(r => new KumiActPaymentEventVM
            {
                Date = (r.DateExecute ?? r.DateIn ?? r.DateDocument).Value,
                Tenancy = r.PaymentCharges.Where(pc => chargeIds.Contains(pc.IdCharge)).Sum(pc => pc.TenancyValue),
                Penalty = r.PaymentCharges.Where(pc => chargeIds.Contains(pc.IdCharge)).Sum(pc => pc.PenaltyValue),
                IdPayment = r.IdPayment,
                NumDocument = r.NumDocument,
                DateDocument = r.DateDocument
            });

            resultPayments = resultPayments.Where(r => r.Date <= atDate);

            var claims = registryContext.Claims.Include(r => r.PaymentClaims).Include(r => r.ClaimStates).Where(r => r.IdAccountKumi == idAccount).ToList();
            var resultClaims = claims.Where(r =>
                    r.EndDeptPeriod <= atDate && (maxBksCharegeDate == null || r.AtDate > maxBksCharegeDate) &&
                    r.ClaimStates.Any(s => s.IdStateType == 4 && s.CourtOrderDate != null) &&
                    !r.ClaimStates.Any(s => s.IdStateType == 6 && s.CourtOrderCancelDate != null))
                .Select(r => new KumiActClaimEventVM
                {
                    Date = r.EndDeptPeriod.Value, 
                    Tenancy = (r.AmountTenancy + r.AmountPkk + r.AmountPadun + r.AmountDgi) ?? 0,
                    Penalty = r.AmountPenalties ?? 0,
                    IdClaim = r.IdClaim,
                    StartDeptPeriod = r.StartDeptPeriod,
                    EndDeptPeriod = r.EndDeptPeriod
                });

            // Копии платежей и ПИР для вычитания сумм
            var paymentsForCalc = resultPayments.Select(r => new KumiActPaymentEventVM
            {
                IdPayment = r.IdPayment,
                Date = r.Date,
                Tenancy = r.Tenancy,
                Penalty = r.Penalty
            }).ToList();
            var claimsForCalc = resultClaims.Select(r => new KumiActClaimEventVM
            {
                IdClaim = r.IdClaim,
                Date = r.Date,
                Tenancy = r.Tenancy,
                Penalty = r.Penalty
            }).ToList();
            // Расчет акта
            var result = new List<KumiActChargeVM>();
            var isFirstCharge = true;
            foreach(var charge in charges.OrderBy(r => r.StartDate))  // Исключить начисления после atDate
            {
                var chargeValue = charge.ChargeTenancy + charge.RecalcTenancy + (isFirstCharge ? charge.InputTenancy : 0);
                var chargeVM = new KumiActChargeVM
                {
                    Value = chargeValue,
                    Date = charge.EndDate,
                    Events = new List<IChargeEventVM>()
                };
                var allPenaltiesCalcEvents = new List<KumiActPeniCalcEventVM>();
                while (true)
                {
                    var firstPayment = paymentsForCalc.Where(r => r.Tenancy > 0).OrderBy(r => r.Date).FirstOrDefault();
                    KumiActPaymentEventVM eventPayment = null;
                    if (firstPayment != null)
                    {
                        eventPayment = resultPayments.Where(r => r.IdPayment == firstPayment.IdPayment)
                            .Select(r => new KumiActPaymentEventVM
                            {
                                IdPayment = r.IdPayment,
                                NumDocument = r.NumDocument,
                                DateDocument = r.DateDocument,
                                Tenancy = r.Tenancy,
                                TenancyTail = firstPayment.Tenancy,
                                Penalty = r.Penalty,
                                PenaltyTail = firstPayment.Penalty,
                                Date = r.Date
                            }).First();
                    }

                    var firstClaim = claimsForCalc.Where(r => r.Tenancy > 0).OrderBy(r => r.Date).FirstOrDefault();
                    KumiActClaimEventVM eventClaim = null;
                    if (firstClaim != null)
                    {
                        eventClaim = resultClaims.Where(r => r.IdClaim == firstClaim.IdClaim)
                            .Select(r => new KumiActClaimEventVM
                            {
                                IdClaim = r.IdClaim,
                                StartDeptPeriod = r.StartDeptPeriod,
                                EndDeptPeriod = r.EndDeptPeriod,
                                Tenancy = r.Tenancy,
                                TenancyTail = firstClaim.Tenancy,
                                PenaltyTail = firstClaim.Penalty,
                                Penalty = r.Penalty,
                                Date = r.Date
                            }).First();
                    }


                    IChargeEventVM currentPaymentEvent = null;
                    if (firstPayment == null && firstClaim == null)
                    {
                        // Платежей и ПИР нет, расчитать пени до atDate
                        if (charge.IsBksCharge == 0)
                        {
                            calculationService.CalcPenalty(chargeVM.Date, atDate, chargeValue, out List<KumiActPeniCalcEventVM> lastPeniCalcEvents);
                            chargeVM.Events.AddRange(lastPeniCalcEvents);
                        }
                        break;
                    } else
                    if (firstPayment != null && firstClaim == null)
                    {
                        chargeVM.Events.Add(eventPayment);
                        currentPaymentEvent = firstPayment;
                    } else
                    if (firstPayment == null && firstClaim != null)
                    {
                        chargeVM.Events.Add(eventClaim);
                        currentPaymentEvent = firstClaim;
                    } else
                    if (firstPayment != null && firstClaim != null)
                    {
                        if (firstPayment.Date > firstClaim.Date)
                        {
                            currentPaymentEvent = firstClaim;
                            chargeVM.Events.Add(eventClaim);
                        } else
                        {
                            currentPaymentEvent = firstPayment;
                            chargeVM.Events.Add(eventPayment);
                        }
                    }

                    var sum = Math.Min(currentPaymentEvent.Tenancy, chargeValue);
                    currentPaymentEvent.Tenancy -= sum;
                    currentPaymentEvent.Penalty = 0;
                    chargeValue -= sum;
                    if (charge.IsBksCharge == 0)
                    {
                        calculationService.CalcPenalty(chargeVM.Date, currentPaymentEvent.Date, sum, out List<KumiActPeniCalcEventVM> peniCalcEvents);
                        allPenaltiesCalcEvents.AddRange(peniCalcEvents);
                    }

                    if (chargeValue == 0)
                        break;
                }

                if (charge.IsBksCharge == 1)
                {
                    chargeVM.IsBksCharge = true;
                    if (charge.ChargePenalty > 0)
                        chargeVM.Events.Add(new KumiActPeniCalcEventVM
                        {
                            EndDate = charge.EndDate,
                            StartDate = charge.StartDate,
                            Tenancy = charge.ChargeTenancy,
                            Penalty = charge.ChargePenalty
                        });
                }
                if (isFirstCharge && (charge.InputPenalty != 0 || charge.InputTenancy != 0))
                {
                    allPenaltiesCalcEvents.Add(new KumiActPeniCalcEventVM
                    {
                        EndDate = charge.EndDate,
                        Tenancy = charge.InputTenancy,
                        Penalty = charge.InputPenalty
                    });
                }

                chargeVM.Events.AddRange(allPenaltiesCalcEvents.GroupBy(r => new { r.StartDate, r.EndDate, r.KeyRate, r.KeyRateCoef })
                    .Select(r => new KumiActPeniCalcEventVM {
                        StartDate = r.Key.StartDate,
                        EndDate = r.Key.EndDate,
                        KeyRate = r.Key.KeyRate,
                        KeyRateCoef = r.Key.KeyRateCoef,
                        Tenancy = r.Select(t => t.Tenancy).Sum(),
                        Penalty = r.Select(p => p.Penalty).Sum()
                    }));
                result.Add(chargeVM);

                isFirstCharge = false;
            }

            return result;
        }

        public string GetUrlForPenaltyCalculator(int idAccount, DateTime? startDate, DateTime? endDate, bool checkPayment)
        {
            var accounts = registryContext.KumiAccounts.Where(r => r.IdAccount == idAccount);
            var accountsPrepare = calculationService.GetAccountsPrepareForPaymentCalculator(accounts);
            var accountsInfo = calculationService.GetAccountInfoForPaymentCalculator(accountsPrepare);
            if (accountsInfo.Count != 1) return null;
            var accountInfo = accountsInfo.First();
            if (endDate == null) endDate = DateTime.MaxValue;

            var resultPayments = calculationService.GetPaymentsInfoForCalcPenalty(accountInfo.Corrections, accountInfo.Charges, accountInfo.Claims, accountInfo.Payments, endDate);
            var resultChargesInfo = calculationService.GetChargesInfoForCalcPenalty(accountInfo.Corrections, accountInfo.Charges, accountInfo.Charges, endDate, endDate.Value);

            if (startDate != null)
            {
                var sliceDate = startDate.Value.AddDays(-11);
                resultPayments = resultPayments.Where(r => r.Date > sliceDate).ToList();
                
                var aggCharges = resultChargesInfo.Where(r => r.Date <= sliceDate);
                resultChargesInfo = resultChargesInfo.Where(r => r.Date > sliceDate).ToList();

                if (aggCharges.Any() && !checkPayment)
                {
                    var dbCharge = accountInfo.Charges.Where(r => r.StartDate <= sliceDate).OrderByDescending(r => r.StartDate).FirstOrDefault();
                    if (dbCharge != null)
                    {
                        if (dbCharge.InputTenancy - dbCharge.PaymentTenancy > 0)
                        {
                            resultChargesInfo.Add(new KumiSumDateInfo
                            {
                                Date = sliceDate,
                                Value = dbCharge.InputTenancy - dbCharge.PaymentTenancy
                            });
                            resultChargesInfo = resultChargesInfo.OrderBy(r => r.Date).ToList();
                        }
                        else
                        if (dbCharge.InputTenancy - dbCharge.PaymentTenancy < 0)
                        {
                            var bufSum = Math.Abs(dbCharge.InputTenancy - dbCharge.PaymentTenancy);
                            while (!(bufSum == 0 || !resultChargesInfo.Any(r => r.Value > 0)))
                            {
                                var charge = resultChargesInfo.Where(r => r.Value > 0).OrderBy(r => r.Date).FirstOrDefault();
                                if (charge != null)
                                {
                                    if (bufSum < charge.Value)
                                    {
                                        charge.Value = charge.Value - bufSum;
                                        bufSum = 0;
                                    }
                                    else
                                    {
                                        bufSum = bufSum - charge.Value;
                                        charge.Value = 0;
                                    }
                                }
                            }
                            resultChargesInfo = resultChargesInfo.Where(r => r.Value > 0).OrderBy(r => r.Date).ToList();
                        }
                    }
                }
            }
            var minChargeDate = resultChargesInfo.Select(r => r.Date).Min();
            var maxChargeDate = resultChargesInfo.Select(r => r.Date).Max();

            var url = "";
            if (resultChargesInfo.Count > 0)
            {
                var firstLoan = resultChargesInfo.OrderBy(r => r.Date).First();
                url += "loanAmount=" + firstLoan.Value.ToString();
                resultChargesInfo.Remove(firstLoan);
            }
            url += "&dateStart="+ minChargeDate.AddDays(11).ToString("dd.MM.yyyy");
            url += "&dateFinish=" + (endDate == DateTime.MaxValue ? maxChargeDate : endDate.Value).ToString("dd.MM.yyyy");
            url += "&rateType=2&back=1&resultView=1";
            if (resultPayments.Count > 0)
            {
                var paymentStr = "";
                foreach (var payment in resultPayments)
                {
                    if (payment.Value == 0) continue;
                    if (!string.IsNullOrEmpty(paymentStr)) paymentStr += ";";
                    paymentStr += string.Format("{0}_{1}_", payment.Date.ToString("dd.MM.yyyy"), payment.Value.ToString());
                }
                url += "&payments=" + paymentStr;
            }
            if (resultChargesInfo.Count > 0)
            {
                var loansStr = "";
                foreach (var charge in resultChargesInfo)
                {
                    if (charge.Value == 0) continue;
                    if (!string.IsNullOrEmpty(loansStr)) loansStr += ";";
                    loansStr += string.Format("{0}_{1}", charge.Date.AddDays(11).ToString("dd.MM.yyyy"), charge.Value.ToString());
                }
                url += "&loans=" + loansStr;
            }
            
            return url;
        }
 
        public KumiAccountsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            KumiAccountsFilter filterOptions, out List<int> filteredIds)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var accounts = GetQuery();
            viewModel.PageOptions.TotalRows = accounts.Count();
            var query = filterService.GetQueryFilter(accounts, viewModel.FilterOptions);
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            query = filterService.GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            filteredIds = query.Select(c => c.IdAccount).ToList();

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = filterService.GetQueryPage(query, viewModel.PageOptions);
            viewModel.Accounts = query.ToList();
            viewModel.TenancyInfo = tenanciesService.GetTenancyInfo(viewModel.Accounts);
            viewModel.ClaimsInfo = claimsService.GetClaimsInfo(viewModel.Accounts);
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "IdRegion", "Region");
            viewModel.BksAccounts = GetBksAccounts(viewModel.Accounts);

            if (filterOptions?.IdRegions != null && filterOptions?.IdRegions.Count > 0)
            {
                var streets = new List<KladrStreet>();
                foreach (var idRegion in filterOptions.IdRegions)
                {
                    streets.AddRange(addressesDataService.GetKladrStreets(idRegion));
                }
                viewModel.KladrStreetsList = new SelectList(streets, "IdStreet", "StreetName");
            }
            else
                viewModel.KladrStreetsList = new SelectList(addressesDataService.GetKladrStreets(null), "IdStreet", "StreetName");

            return viewModel;
        }

        private IEnumerable<PaymentAccount> GetBksAccounts(IEnumerable<KumiAccount> accounts)
        {
            var ids = accounts.Select(r => r.IdAccount).ToList();
            return registryContext.PaymentAccounts.Where(pa => ids.Contains(pa.IdAccount)).ToList();
        }

        public KumiAccountsVM GetAccountsViewModelForMassReports(List<int> ids, PageOptions pageOptions)
        {
            var viewModel = InitializeViewModel(null, pageOptions, null);
            var accounts = GetQuery().Where(r => ids.Contains(r.IdAccount));
            viewModel.PageOptions.TotalRows = accounts.Count();
            var count = accounts.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;

            accounts = filterService.GetQueryPage(accounts, viewModel.PageOptions);
            viewModel.Accounts = accounts.ToList();
            viewModel.TenancyInfo = tenanciesService.GetTenancyInfo(viewModel.Accounts);
            viewModel.ClaimsInfo = claimsService.GetClaimsInfo(viewModel.Accounts);

            return viewModel;
        }

        public IQueryable<KumiAccount> GetAccountsForMassReports(List<int> ids)
        {
            return GetQuery().Where(p => ids.Contains(p.IdAccount)).AsNoTracking();
        }

        public Dictionary<int, List<string>> GetTenantsEmails(List<KumiAccount> accounts)
        {
            var emailsDic = new Dictionary<int, List<string>>();
            var emailsInfo = registryContext.GetTenantsByAccountIds(accounts.Select(r => r.IdAccount).ToList());
            foreach (var account in accounts)
            {
                var emailsStr = emailsInfo.FirstOrDefault(r => r.IdAccount == account.IdAccount)?.Emails;
                List<string> emails = new List<string>();
                if (!string.IsNullOrEmpty(emailsStr))
                {
                    emails.AddRange(emailsStr.Split(','));
                }
                emails = emails.Distinct().ToList();
                emailsDic.Add(account.IdAccount, emails);
            }
            return emailsDic;
        }

        public IQueryable<KumiAccount> GetKumiAccounts(KumiAccountsFilter filterOptions)
        {
            var kumiAccounts = GetQuery();
            var query = filterService.GetQueryFilter(kumiAccounts, filterOptions);
            return query;
        }

        private IQueryable<KumiAccount> GetQuery()
        {
            return registryContext.KumiAccounts
                .Include(r => r.AccountsTenancyProcessesAssoc)
                .Include(r => r.Claims)
                .Include(r => r.Charges)
                .Include(r => r.State);
        }
       
        public KumiAccount GetKumiAccount(int idAccount, bool withHiddenCharges = false)
        {
            var account = registryContext.KumiAccounts
                .Include(r => r.AccountsTenancyProcessesAssoc)
                .Include(r => r.Charges)
                .Include(r => r.Claims)
                .Include(r => r.Corrections)
                .SingleOrDefault(a => a.IdAccount == idAccount);
            if (account == null) return null;
            foreach(var claim in account.Claims)
            {
                var currentClaimState = registryContext.ClaimStates.Include(r => r.IdStateTypeNavigation).Where(r => r.IdClaim == claim.IdClaim);
                claim.ClaimStates = currentClaimState.ToList();
            }
            account.Charges = account.Charges.Where(r => withHiddenCharges || r.Hidden != 1).ToList();
            foreach (var charge in account.Charges)
            {
                var paymentCharges = registryContext.KumiPaymentCharges.Where(r => r.IdCharge == charge.IdCharge);
                charge.PaymentCharges = paymentCharges.ToList();
                charge.DisplayPaymentClaims = registryContext.KumiPaymentClaims.Where(r => r.IdDisplayCharge == charge.IdCharge).ToList();
                charge.UntiedPaymentsInfo = registryContext.KumiPaymentsUntied.Where(r => r.IdCharge == charge.IdCharge).ToList();
            }

            return account;
        }

        public void Create(KumiAccount account)
        {
            var accountTenancyAssocs = account.AccountsTenancyProcessesAssoc;
            account.Charges = null;
            account.Claims = null;
            account.AccountsTenancyProcessesAssoc = null;
            account.State = null;
            registryContext.KumiAccounts.Add(account);
            registryContext.SaveChanges();
            UpdateAccountTenancyAssocs(accountTenancyAssocs, account.IdAccount);
            registryContext.SaveChanges();
        }
        public void Edit(KumiAccount account)
        {
            var accountTenancyAssocs = account.AccountsTenancyProcessesAssoc;
            account.Charges = null;
            account.Claims = null;
            account.AccountsTenancyProcessesAssoc = null;
            account.State = null;
            if (account.IdState == 2)
            {
                account.RecalcMarker = 0;
                account.RecalcReason = null;
            }
            registryContext.KumiAccounts.Update(account);
            UpdateAccountTenancyAssocs(accountTenancyAssocs, account.IdAccount);
            registryContext.SaveChanges();
        }

        private void UpdateAccountTenancyAssocs(IList<KumiAccountsTenancyProcessesAssoc> accountTenancyAssocs, int idAccount)
        {
            var oldAccountTenancyAssocs = registryContext.KumiAccountsTenancyProcessesAssocs.Where(r => r.IdAccount == idAccount);
            foreach(var oldAssoc in oldAccountTenancyAssocs)
            {
                if (accountTenancyAssocs != null && accountTenancyAssocs.Count(r => r.IdAssoc == oldAssoc.IdAssoc) > 0) continue;
                oldAssoc.Deleted = 1;
            }
            if (accountTenancyAssocs == null) return;
            foreach (var newAssoc in accountTenancyAssocs)
            {
                if (newAssoc.IdAssoc != 0)
                {
                    var assocDb = registryContext.KumiAccountsTenancyProcessesAssocs.FirstOrDefault(r => r.IdAssoc == newAssoc.IdAssoc);
                    assocDb.Fraction = newAssoc.Fraction;
                    assocDb.IdProcess = newAssoc.IdProcess;
                    assocDb.IdAccount = idAccount;
                    registryContext.KumiAccountsTenancyProcessesAssocs.Update(assocDb);
                } else
                {
                    newAssoc.IdAccount = idAccount;
                    registryContext.KumiAccountsTenancyProcessesAssocs.Add(newAssoc);
                }
            }
        }

        public void Delete(int idAccount)
        {
            var account = registryContext.KumiAccounts
                .Include(pc => pc.Charges)
                .FirstOrDefault(pc => pc.IdAccount == idAccount);
            if (account.Charges.Any()) throw new Exception("Нельзя удалить лицевой счет, по которому имеются начисления");
            account.Deleted = 1;
            registryContext.SaveChanges();
        }

        public bool AccountExists(string account, int idAccount)
        {
            var curAccount = registryContext.KumiAccounts
                .SingleOrDefault(a => a.IdAccount == idAccount)
                ?.Account;
            if (curAccount == account)
                return false;
            return registryContext.KumiAccounts
                .Select(a => a.Account).Count(num => num != null && num == account) > 0;
        }

        public List<KumiAccountState> States { get => registryContext.KumiAccountStates.ToList(); }
        public List<KladrStreet> Streets { get => addressesDataService.KladrStreets.ToList(); }
        public List<KladrRegion> Regions { get => addressesDataService.KladrRegions.ToList(); }

        public List<KumiKeyRate> GetKeyRates()
        {
            var keyRatesList = registryContext.KumiKeyRates
                                    .Where(k=>k.StartDate<=DateTime.Parse("14.02.2022") || k.StartDate >= DateTime.Parse("19.09.2022"))
                                    .ToList();

            keyRatesList.Add(new KumiKeyRate
            {
                StartDate=DateTime.Parse("01.01.2999"),
                Value=0
            });

            keyRatesList.Add(new KumiKeyRate
            {
                StartDate=DateTime.Parse("01.08.2022"),
                Value=(Decimal)8.00
            });

            return keyRatesList.OrderByDescending(k=>k.StartDate).ToList();
        }
    }
}
