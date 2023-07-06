using RegistryDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities;
using RegistryDb.Models.SqlViews;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.Enums;
using RegistryDb.Models.Entities.Claims;
using RegistryServices.Models;
using RegistryDb.Models.Entities.Common;
using RegistryServices.Models.KumiAccounts;

namespace RegistryWeb.DataServices
{
    public class KumiAccountsDataService : ListDataService<KumiAccountsVM, KumiAccountsFilter>
    {
        private readonly SecurityServices.SecurityService securityService;

        public KumiAccountsDataService(RegistryContext registryContext, AddressesDataService addressesDataService,
            SecurityServices.SecurityService securityService) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
        }

        public override KumiAccountsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiAccountsFilter filterOptions)
        {
            var vm = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            vm.AccountStates = new SelectList(registryContext.KumiAccountStates.ToList(), "IdState", "State");
            return vm;
        }


        public DateTime? GetAccountStartCalcDate(KumiAccountInfoForPaymentCalculator account)
        {
            DateTime? result = null;
            if (!account.Charges.Any())
            {
                if (account.LastChargeDate != null)
                    result = account.LastChargeDate.Value.AddDays(-account.LastChargeDate.Value.Day + 1).AddMonths(1);
                else
                {
                    result = StartCalcDateFromRentPeriods(account.TenancyInfo);
                }
            }
            else
            {
                var firstCharge = account.Charges.OrderBy(r => r.EndDate).First();
                if (firstCharge.IsBksCharge == 1)
                {
                    result = firstCharge.StartDate;
                } else
                {
                    // TODO min_date start work registry
                    var rentStartDate = StartCalcDateFromRentPeriods(account.TenancyInfo);
                    if (rentStartDate == null) return firstCharge.StartDate;
                    result = firstCharge.StartDate < rentStartDate ? firstCharge.StartDate : rentStartDate;
                }
            }
            if (result == null) return null;
            return result;
        }

        private DateTime? StartCalcDateFromRentPeriods(List<KumiTenancyInfoForPaymentCalculator> tenancyInfo)
        {
            var startRentDate = tenancyInfo.SelectMany(r => r.RentPeriods).Select(r => r.FromDate).Min();
            if (startRentDate == null) return null;
            return startRentDate.Value.AddDays(-startRentDate.Value.Day + 1);
        }

        public List<KumiCharge> CalcChargesInfo(KumiAccountInfoForPaymentCalculator account, List<KumiCharge> dbChargers, DateTime startDate, DateTime endDate, DateTime startRewriteDate, bool forceCalcChargeOnFutureDate = false)
        {
            if (startDate > endDate) throw new ApplicationException(
                string.Format("Дата начала расчета {0} превышает дату окончания завершенного периода {1}",
                startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy")));

            var charges = new List<KumiCharge>();

            if (dbChargers.Any(r => r.IsBksCharge == 1))
            {
                startDate = dbChargers.Where(r => r.IsBksCharge == 1).Select(r => r.EndDate).Max().AddDays(1);
                charges.AddRange(dbChargers.Where(r => r.IsBksCharge == 1).Select(r => new KumiCharge {
                    IdAccount = r.IdAccount,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ChargeTenancy = r.ChargeTenancy,
                    ChargePenalty = r.ChargePenalty,
                    InputTenancy = r.InputTenancy,
                    InputPenalty = r.InputPenalty,
                    OutputTenancy = r.OutputTenancy,
                    OutputPenalty = r.OutputPenalty,
                    PaymentTenancy = r.PaymentTenancy,
                    PaymentPenalty = r.PaymentPenalty,
                    RecalcTenancy = r.RecalcTenancy,
                    RecalcPenalty = r.RecalcPenalty,
                    IsBksCharge = r.IsBksCharge,
                }));
            }

            while(startDate < endDate)
            {
                var subEndDate = startDate.AddMonths(1).AddDays(-1);
                var charge = CalcChargeInfo(account, startDate, subEndDate, charges, startRewriteDate, out bool outOfBound, forceCalcChargeOnFutureDate);
                if (!outOfBound)
                    charges.Add(charge);
                startDate = startDate.AddMonths(1);
            }
            return charges;
        }

        public void CalcRecalcInfo(KumiAccountInfoForPaymentCalculator account, List<KumiCharge> chargingInfo, 
            List<KumiCharge> dbChargingInfo, KumiCharge recalcInsertIntoCharge,
            DateTime startCalcDate, DateTime endCalcDate, DateTime startRewriteDate)
        {
            var accRecalcTenancy = 0m;
            var accRecalcPenalty = 0m;
            var accCorrectionTenancy = 0m;
            var accCorrectionPenalty = 0m;
            var accPaymentTenancy = 0m;
            var accPaymentPenalty = 0m;
            foreach (var dbCharge in dbChargingInfo)
            {
                if (dbCharge.IsBksCharge == 1) continue;

                // Если начисление в периоде перезаписи или вне периода расчета, то пропускаем
                if (startRewriteDate <= dbCharge.StartDate || endCalcDate < dbCharge.StartDate)
                {
                    continue;
                }
                
                var charge = chargingInfo.FirstOrDefault(r => r.StartDate == dbCharge.StartDate && r.EndDate == dbCharge.EndDate);

                // Если в новом расчете нет начисления, а в базе есть, но начисление не перезаписываемое, то пересчитываем его в доплату
                if (charge == null)
                {
                    accRecalcPenalty -= dbCharge.OutputPenalty - dbCharge.InputPenalty;
                    accRecalcTenancy -= dbCharge.OutputTenancy - dbCharge.InputTenancy;
                }
            }
            
            var sortedChargingInfo = chargingInfo.OrderBy(r => r.EndDate).ToList();
            for (var i = 0; i < sortedChargingInfo.Count; i++)
            {
                var charge = sortedChargingInfo[i];
                // Если начисление перезаписываемое, то пропускаем, т.к. перерасчет по нему встанет как положено и нет необходимости в переносе на текущий период
                if (startRewriteDate < charge.StartDate)
                {
                    continue;
                }

                // Если в БД есть начисление, но оно перенесено от БКС, то пропускаем, т.к. эти начисления считаются неперезаписываемыми
                var dbCharge = dbChargingInfo.FirstOrDefault(r => r.StartDate == charge.StartDate && r.EndDate == charge.EndDate);
                if (dbCharge != null && dbCharge.IsBksCharge == 1) continue;

                var dbNextCharge = dbChargingInfo.OrderBy(r => r.EndDate).FirstOrDefault(r => r.EndDate > charge.EndDate);
                var nextCharge = sortedChargingInfo.OrderBy(r => r.EndDate).FirstOrDefault(r => r.EndDate > charge.EndDate);

                // Если в БД нет соответствующего начисления, но это начисление не является последним в БД, то добавляем сумму в перерасчет
                if (dbCharge == null && dbNextCharge != null && startRewriteDate != charge.StartDate)
                {
                    accRecalcPenalty += charge.OutputPenalty - charge.InputPenalty;
                    accRecalcTenancy += charge.OutputTenancy - charge.InputTenancy;
                }
                else
                // Признак Hidden присваивается заблокированным периодам, по которым производилась привязка платежей без начислений
                // Такие лицевые счета надо перерасчитывать полностью, чтобы отобразить платеж. В противном случае платеж пойдет в перерасчет
                if (dbCharge != null && dbCharge.Hidden == 1 && startRewriteDate != charge.StartDate)
                {
                    accPaymentTenancy += charge.PaymentTenancy;
                    accPaymentPenalty += charge.PaymentPenalty;
                }
                else
                // Если в БД есть начисление, но в расчете есть начисления после него, то аккумулируем разницу платежей, перерасчета и корректировок
                // для переноса на последнее начисление
                if (dbCharge != null && nextCharge != null)
                {
                    if (startRewriteDate != charge.StartDate)
                    {
                        accRecalcTenancy += charge.ChargeTenancy - dbCharge.ChargeTenancy;
                        accRecalcPenalty += charge.ChargePenalty - dbCharge.ChargePenalty;
                        accCorrectionTenancy += charge.CorrectionTenancy - dbCharge.CorrectionTenancy;
                        accCorrectionPenalty += charge.CorrectionPenalty - dbCharge.CorrectionPenalty;
                        accPaymentTenancy += charge.PaymentTenancy - dbCharge.PaymentTenancy;
                        accPaymentPenalty += charge.PaymentPenalty - dbCharge.PaymentPenalty;
                    }
                    accRecalcTenancy -= dbCharge.RecalcTenancy;
                    accRecalcPenalty -= dbCharge.RecalcPenalty;
                }
            }

            recalcInsertIntoCharge.RecalcTenancy = accRecalcTenancy;
            recalcInsertIntoCharge.RecalcPenalty = accRecalcPenalty;
            recalcInsertIntoCharge.CorrectionTenancy = accCorrectionTenancy;
            recalcInsertIntoCharge.CorrectionPenalty = accCorrectionPenalty;
            recalcInsertIntoCharge.PaymentTenancy += accPaymentTenancy;
            recalcInsertIntoCharge.PaymentPenalty += accPaymentPenalty;
        }

        public DateTime CorrectStartRewriteDate(DateTime startRewriteDate, DateTime startDate, List<KumiCharge> dbChargingInfo)
        {
            if (dbChargingInfo.Where(r => r.Hidden == 0).Any())
            {
                var lastChargeEndDate = dbChargingInfo.Where(r => r.Hidden == 0).OrderBy(r => r.EndDate).Last().EndDate;
                var maxStartRewriteDate = lastChargeEndDate.AddDays(1);
                if (maxStartRewriteDate < startRewriteDate) startRewriteDate = maxStartRewriteDate;
            }
            if (dbChargingInfo.Any(r => r.IsBksCharge == 1))
            {
                var minStartRewriteDate = dbChargingInfo.Where(r => r.IsBksCharge == 1).OrderBy(r => r.EndDate).Last().EndDate.AddDays(1);
                if (startRewriteDate < minStartRewriteDate) startRewriteDate = minStartRewriteDate;
            }
            return startRewriteDate;
        }

        public void AddChargeCorrection(int idAccount, decimal tenancyValue, decimal penaltyValue, DateTime atDate, string description)
        {
            registryContext.KumiChargeCorrections.Add(new KumiChargeCorrection {
                IdAccount = idAccount,
                Sum = sum,
                Date = atDate,
                Description = description
            });
            registryContext.SaveChanges();
        }

        public List<KumiAccount> GetAccountsForTenancies(List<TenancyProcess> tenancies)
        {
            var tenanciIds = tenancies.Select(r => r.IdProcess);
            var accounts = from row in registryContext.KumiAccounts.Include(r => r.State).Include(r => r.AccountsTenancyProcessesAssoc)
                           where row.AccountsTenancyProcessesAssoc.Count(r => tenanciIds.Contains(r.IdProcess)) > 0
                           select row;
            return accounts.ToList();
        }

        public void RecalculateAccounts(List<int> accountIds, KumiAccountRecalcTypeEnum recalcType, DateTime? recalcStartDate)
        {
            var accounts = registryContext.KumiAccounts.Where(r => accountIds.Contains(r.IdAccount) && r.IdState != 2);

            if (accounts.Count() == 0) return;

            DateTime? startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.Value.AddDays(-startRewriteDate.Value.Day + 1);

            var endCalcDate = DateTime.Now.Date;
            endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);

            if (DateTime.Now.Date.Day >= 25) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.Value.AddMonths(1);
                endCalcDate = endCalcDate.AddDays(1).AddMonths(1).AddDays(-1);
            }
            if (recalcType == KumiAccountRecalcTypeEnum.RewriteCharge)
            {
                startRewriteDate = recalcStartDate;
            }

            RecalculateAccounts(accounts, startRewriteDate, endCalcDate);
        }

        public KumiCharge CalcForecastChargeInfo(int idAccount, DateTime calcToDate)
        {
            DateTime? startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.Value.AddDays(-startRewriteDate.Value.Day + 1);

            var accounts = registryContext.KumiAccounts.Where(r => r.IdAccount == idAccount);
            var accountsPrepare = GetAccountsPrepareForPaymentCalculator(accounts);
            var accountsInfo = GetAccountInfoForPaymentCalculator(accountsPrepare).ToList();
            var account = accountsInfo.FirstOrDefault();
            if (account == null) return new KumiCharge();

            var startCalcDate = GetAccountStartCalcDate(account);
            if (startCalcDate == null) return new KumiCharge();

            var dbChargingInfo = GetDbChargingInfo(account);

            if (startRewriteDate == null)
                startRewriteDate = startCalcDate;

            var chargingInfo = CalcChargesInfo(account, dbChargingInfo, startCalcDate.Value, calcToDate, startRewriteDate.Value, true);
            var recalcInsertIntoCharge = new KumiCharge();
            if (chargingInfo.Any()) recalcInsertIntoCharge = chargingInfo.Last();
            startRewriteDate = CorrectStartRewriteDate(startRewriteDate.Value, startCalcDate.Value, dbChargingInfo);
            CalcRecalcInfo(account, chargingInfo, dbChargingInfo, recalcInsertIntoCharge, startCalcDate.Value, calcToDate, startRewriteDate.Value);
            return chargingInfo.FirstOrDefault(r => r.EndDate == calcToDate) ?? new KumiCharge();
        }

        public void RecalculateAccounts(IQueryable<KumiAccount> accounts, DateTime? startRewriteDate, DateTime endCalcDate)
        {
            var accountsPrepare = GetAccountsPrepareForPaymentCalculator(accounts);
            var accountsInfo = GetAccountInfoForPaymentCalculator(accountsPrepare);

            foreach (var account in accountsInfo)
            {
                var startCalcDate = GetAccountStartCalcDate(account);
                if (startCalcDate == null) continue;
                if (startRewriteDate == null)
                    startRewriteDate = startCalcDate;

                var dbChargingInfo = GetDbChargingInfo(account);

                // Если было начисление на текущий или будущий период, то сохранить его при перерасчете (по-умолчанию начислеяются только закрытыие периоды)
                var forceCalcChargeOnFutureDate = false;
                var lastDbCharge = dbChargingInfo.OrderByDescending(r => r.EndDate).FirstOrDefault();
                if (lastDbCharge != null && lastDbCharge.EndDate >= DateTime.Now.Date)
                {
                    forceCalcChargeOnFutureDate = true;
                    endCalcDate = lastDbCharge.EndDate;
                }

                var chargingInfo = CalcChargesInfo(account, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate.Value, forceCalcChargeOnFutureDate);
                var recalcInsertIntoCharge = new KumiCharge();
                if (chargingInfo.Any()) recalcInsertIntoCharge = chargingInfo.Last();

                startRewriteDate = CorrectStartRewriteDate(startRewriteDate.Value, startCalcDate.Value, dbChargingInfo);
                CalcRecalcInfo(account, chargingInfo, dbChargingInfo, recalcInsertIntoCharge, startCalcDate.Value, endCalcDate, startRewriteDate.Value);
                UpdateChargesIntoDb(account, chargingInfo, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate.Value, null);
            }
        }

        public List<KumiCharge> GetDbChargingInfo(KumiAccountInfoForPaymentCalculator account)
        {
            return registryContext.KumiCharges.AsNoTracking().Where(r => r.IdAccount == account.IdAccount).ToList();
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
                            CalcPenalty(chargeVM.Date, atDate, chargeValue, out List<KumiActPeniCalcEventVM> lastPeniCalcEvents);
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
                        CalcPenalty(chargeVM.Date, currentPaymentEvent.Date, sum, out List<KumiActPeniCalcEventVM> peniCalcEvents);
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

        public void UpdateChargesIntoDb(KumiAccountInfoForPaymentCalculator account, List<KumiCharge> chargingInfo,
            List<KumiCharge> dbChargingInfo,
            DateTime startCalcDate, DateTime endCalcDate, DateTime startRewriteDate, DateTime? calcDate)
        {
            var actualDbCharges = dbChargingInfo.ToList();
            KumiCharge firstDbRewriteCharge = null;
            foreach (var dbCharge in dbChargingInfo.OrderBy(r => r.EndDate))
            {
                var charge = chargingInfo.FirstOrDefault(r => r.StartDate == dbCharge.StartDate && r.EndDate == dbCharge.EndDate);
                if (charge != null)
                    charge.Hidden = dbCharge.Hidden;

                // Если начисление вне периода перезаписи или вне периода расчета, то пропускаем
                if (startRewriteDate > dbCharge.StartDate || endCalcDate < dbCharge.StartDate)
                {
                    continue;
                }

                // Если начисление в периоде перезаписи, но не найдено соответствующее расчетное начисление, то удаляем начисление из БД
                if (firstDbRewriteCharge == null)
                    firstDbRewriteCharge = dbCharge;

                if (charge == null)
                {
                    foreach (var paymentCharge in dbCharge.PaymentCharges)
                    {
                        dbCharge.PaymentCharges.Remove(paymentCharge);
                    }
                    registryContext.KumiCharges.Remove(dbCharge);
                    actualDbCharges.Remove(dbCharge);
                }
            }

            var currentTenancy = 0m;
            var currentPenalty = 0m;
            DateTime? lastChargingDate = null;
            var sortedChargingInfo = chargingInfo.OrderBy(r => r.EndDate).ToList();

            var lastDbLockedCharge = actualDbCharges.Where(r => r.Hidden == 0).OrderByDescending(r => r.EndDate)
                .FirstOrDefault(r => r.EndDate < startRewriteDate);

            for (var i = 0; i < sortedChargingInfo.Count; i++)
            {
                var charge = sortedChargingInfo[i];

                // Если начисление вне периода перезаписи, то пропускаем
                if (startRewriteDate > charge.StartDate)
                {
                    continue;
                }

                var dbCharge = actualDbCharges.FirstOrDefault(r => r.StartDate == charge.StartDate && r.EndDate == charge.EndDate);

                var prevCharge = sortedChargingInfo.Where(r => r.Hidden == 0).OrderByDescending(r => r.EndDate).FirstOrDefault(r => r.EndDate < charge.EndDate);            
                var nextCharge = sortedChargingInfo.Where(r => r.Hidden == 0).OrderBy(r => r.EndDate).FirstOrDefault(r => r.EndDate > charge.EndDate);

                charge.PaymentCharges = null;
                if (lastDbLockedCharge == null)
                {
                    if (prevCharge == null)
                    {
                        charge.InputTenancy = firstDbRewriteCharge?.InputTenancy ?? charge.InputTenancy;
                        charge.InputPenalty = firstDbRewriteCharge?.InputPenalty ?? charge.InputTenancy;
                    } else
                    {
                        charge.InputTenancy = prevCharge.OutputTenancy;
                        charge.InputPenalty = prevCharge.OutputPenalty;
                    }
                } else
                if (prevCharge != null)
                {
                    if (lastDbLockedCharge.StartDate >= prevCharge.StartDate)
                    {
                        charge.InputTenancy = lastDbLockedCharge.OutputTenancy;
                        charge.InputPenalty = lastDbLockedCharge.OutputPenalty;
                        lastDbLockedCharge = null;
                    } else
                    {
                        charge.InputTenancy = prevCharge.OutputTenancy;
                        charge.InputPenalty = prevCharge.OutputPenalty;
                    }
                }

                if (startRewriteDate < charge.StartDate && nextCharge != null)
                {
                    charge.RecalcTenancy = 0;
                    charge.RecalcPenalty = 0;
                }
                charge.OutputTenancy = charge.InputTenancy + charge.ChargeTenancy - charge.PaymentTenancy + charge.RecalcTenancy;
                charge.OutputPenalty = charge.InputPenalty + charge.ChargePenalty - charge.PaymentPenalty + charge.RecalcPenalty;

                var notAddCharge = nextCharge == null && charge.PaymentTenancy == 0 && charge.PaymentPenalty == 0 && charge.ChargeTenancy == 0 &&
                            charge.ChargePenalty == 0 && charge.RecalcTenancy == 0 && charge.RecalcPenalty == 0;

                if (dbCharge == null)
                {
                    if (!notAddCharge)
                        registryContext.KumiCharges.Add(charge);
                }
                else
                {
                    charge.IdCharge = dbCharge.IdCharge;
                    if (notAddCharge)
                    {
                            registryContext.Remove(charge);
                            actualDbCharges.Remove(dbCharge);
                    } else
                    if (charge != dbCharge || charge.Hidden == 1)
                    {
                        charge.Hidden = 0;
                        dbCharge.Hidden = 0;
                        registryContext.Update(charge);
                    }
                }
                if (nextCharge == null)
                {
                    currentTenancy = charge.OutputTenancy;
                    currentPenalty = charge.OutputPenalty;
                    lastChargingDate = chargingInfo.Where(r => r.ChargePenalty != 0 || r.ChargeTenancy != 0).OrderByDescending(r => r.EndDate).FirstOrDefault()?.EndDate;
                }
            }
            var accountDb = registryContext.KumiAccounts.FirstOrDefault(r => r.IdAccount == account.IdAccount);
            accountDb.CurrentBalanceTenancy = currentTenancy;
            accountDb.CurrentBalancePenalty = currentPenalty;
            accountDb.LastChargeDate = lastChargingDate;
            if (calcDate != null)
                accountDb.LastCalcDate = calcDate;
            else
                accountDb.LastCalcDate = (accountDb.LastCalcDate != null && accountDb.LastCalcDate > DateTime.Now.Date) ? accountDb.LastCalcDate : DateTime.Now.Date;
            accountDb.RecalcMarker = 0;
            accountDb.RecalcReason = null;
            registryContext.SaveChanges();
            UpdateChargeDisplayInfo(accountDb.IdAccount, startRewriteDate);
        }

        private void UpdateChargeDisplayInfo(int idAccount, DateTime startRewriteDate)
        {
            var charges = registryContext.KumiCharges.Include(r => r.PaymentCharges).Where(r => r.IdAccount == idAccount);
            // Если строк начисления нет в БД, то нечего обновлять
            if (charges.Count() == 0) return;
            var lastCharge = charges.OrderByDescending(r => r.EndDate).First();
            var claims = registryContext.Claims.Include(r => r.PaymentClaims).Where(r => r.IdAccountKumi == idAccount);
            foreach(var charge in charges)
            {
                foreach(var paymentCharge in charge.PaymentCharges)
                {
                    // Если начисление до даты перезаписи и указан IdDisplayCharge, то оставляем как есть
                    // Если начисление до даты перезаписи и IdDisplayCharge == null, то ставим идентификатор последнего начисления
                    if (paymentCharge.IdDisplayCharge == null && charge.StartDate < startRewriteDate)
                    {
                        paymentCharge.IdDisplayCharge = lastCharge.IdCharge;
                    } else
                    // Если начисление после даты перезаписи, то ставим идентификатор текущего начисления
                    if (charge.StartDate >= startRewriteDate)
                    {
                        paymentCharge.IdDisplayCharge = paymentCharge.IdCharge;
                    }

                }
            }
            var paymentIds = claims.SelectMany(r => r.PaymentClaims).Select(r => r.IdPayment).Distinct();
            var payments = registryContext.KumiPayments.Where(r => paymentIds.Contains(r.IdPayment));
            foreach(var claim in claims)
            {
                foreach(var paymentClaim in claim.PaymentClaims)
                {
                    var payment = payments.First(r => r.IdPayment == paymentClaim.IdPayment);
                    var date = payment.DateExecute ?? payment.DateIn ?? payment.DateDocument;
                    if (date == null) continue;
                    var charge = charges.FirstOrDefault(r => r.StartDate <= date && r.EndDate >= date);
                    if (charge == null) continue;
                    // Если начисление до даты перезаписи и указан IdDisplayCharge, то оставляем как есть
                    // Если начисление до даты перезаписи и IdDisplayCharge == null, то ставим идентификатор последнего начисления
                    if (paymentClaim.IdDisplayCharge == null && charge.StartDate < startRewriteDate)
                    {
                        paymentClaim.IdDisplayCharge = lastCharge.IdCharge;
                    }
                    else
                    // Если начисление после даты перезаписи, то ставим идентификатор текущего начисления
                    if (charge.StartDate >= startRewriteDate)
                    {
                        paymentClaim.IdDisplayCharge = charge.IdCharge;
                    }
                }
            }
            registryContext.SaveChanges();
        }

        public void CreateClaimMass(List<int> accountIds, DateTime atDate)
        {
            var accounts = GetAccountsForMassReports(accountIds).ToList();
            foreach (var account in accounts)
            {
                var claim = new Claim
                {
                    AtDate = atDate,
                    IdAccountKumi = account.IdAccount,
                    AmountTenancy = account.CurrentBalanceTenancy,
                    AmountPenalties = account.CurrentBalancePenalty,
                    AmountDgi = 0,
                    AmountPadun = 0,
                    AmountPkk = 0,
                    ClaimStates = new List<ClaimState> {
                        new ClaimState {
                            IdStateType = registryContext.ClaimStateTypes.Where(r => r.IsStartStateType).First().IdStateType,
                            BksRequester = CurrentExecutor?.ExecutorName,
                            DateStartState = DateTime.Now.Date,
                            Executor = CurrentExecutor?.ExecutorName
                        }
                    },
                    ClaimPersons = new List<ClaimPerson>()
                };
                claim.ClaimPersons = GetClaimPersonsFromTenancy(claim.IdAccountKumi);
                if (claim.ClaimPersons.Count == 0)
                {
                    claim.ClaimPersons = GetClaimPersonsFromPrevClaim(claim.IdAccountKumi);
                }
                
                registryContext.Claims.Add(claim);
                registryContext.SaveChanges();
            }
        }

        private List<ClaimPerson> GetClaimPersonsFromPrevClaim(int? idAccount)
        {
            var prevClaim = registryContext.Claims.Include(r => r.ClaimPersons)
                .Where(r => idAccount != null ? r.IdAccountKumi == idAccount : false)
                .OrderByDescending(r => r.IdClaim).FirstOrDefault();
            if (prevClaim != null)
            {
                return prevClaim.ClaimPersons.Select(r => new ClaimPerson
                {
                    Surname = r.Surname,
                    Name = r.Name,
                    Patronymic = r.Patronymic,
                    DateOfBirth = r.DateOfBirth,
                    Passport = r.Passport,
                    PlaceOfBirth = r.PlaceOfBirth,
                    WorkPlace = r.WorkPlace,
                    IsClaimer = r.IsClaimer
                }).ToList();
            }
            return new List<ClaimPerson>();
        }

        private List<ClaimPerson> GetClaimPersonsFromTenancy(int? idAccount)
        {
            var tenancyPersons = new List<TenancyPerson>();
            if (idAccount != null)
            {
                var tenancyProcesses = (from tpRow in registryContext.TenancyProcesses.Include(tp => tp.AccountsTenancyProcessesAssoc)
                                    .Where(tp => (tp.RegistrationNum == null || !tp.RegistrationNum.Contains("н")) && 
                                         tp.AccountsTenancyProcessesAssoc.Count(atpa => atpa.IdAccount == idAccount) > 0)
                                        select tpRow).ToList();
                if (tenancyProcesses.Any())
                {
                    var idProcess = tenancyProcesses.OrderByDescending(tp => new { tp.RegistrationDate, tp.IdProcess }).First().IdProcess;
                    tenancyPersons = registryContext.TenancyPersons.Where(tp => tp.IdProcess == idProcess && tp.ExcludeDate == null).ToList();
                }
            }
            return tenancyPersons.Select(r => new ClaimPerson
            {
                Surname = r.Surname,
                Name = r.Name,
                Patronymic = r.Patronymic,
                DateOfBirth = r.DateOfBirth,
                IsClaimer = r.IdKinship == 1
            }).ToList();
        }

        public KumiCharge CalcChargeInfo(KumiAccountInfoForPaymentCalculator account, DateTime startDate, DateTime endDate,
            List<KumiCharge> prevCharges, DateTime startRewriteDate, out bool outOfBound, bool forceCalcChargeOnFutureDate)
        {
            var inputBalanceTenancy = account.CurrentBalanceTenancy;
            var inputBalancePenalty = account.CurrentBalancePenalty;

            KumiCharge prevCharge = null;
            if (prevCharges.Any())
                prevCharge = prevCharges.OrderByDescending(r => r.EndDate).First();

            if (prevCharge != null)
            {
                inputBalanceTenancy = prevCharge.OutputTenancy;
                inputBalancePenalty = prevCharge.OutputPenalty;
            } else
            {
                var dbCharge = registryContext.KumiCharges.AsNoTracking()
                    .FirstOrDefault(r => r.IdAccount == account.IdAccount && r.StartDate == startDate && r.EndDate == endDate);
                if (dbCharge != null)
                {
                    inputBalanceTenancy = dbCharge.InputTenancy;
                    inputBalancePenalty = dbCharge.InputPenalty;
                }
            }

            var recalcTenancy = 0m;
            var recalcPenalty = 0m;

            var chargeTenancy = 0m;
            var chargePenalty = 0m;

            KumiCharge currentSavedCharge = account.Charges.FirstOrDefault(r => r.StartDate == startDate && r.EndDate == endDate);
            if (currentSavedCharge != null)
            {
                recalcTenancy = currentSavedCharge.RecalcTenancy;
                recalcPenalty = currentSavedCharge.RecalcPenalty;
                // Если лицевой счет не находится в статусе "Действующий", то переносим начисления, которые были сохранены в БД
                // Игнорируем будущие периоды
                if ((endDate < DateTime.Now.Date || forceCalcChargeOnFutureDate) && account.IdState != 1)
                {
                    chargeTenancy = currentSavedCharge.ChargeTenancy;
                    chargePenalty = currentSavedCharge.ChargePenalty;
                }
            }

            var correctionTenancy = 0m;
            var correctionPenalty = 0m;
            // Если есть корректировки, учитываем их
            var corrections = account.Corrections.Where(r => r.Date >= startDate && r.Date <= endDate);
            correctionTenancy = corrections.Select(r => r.TenancyValue).Sum();
            correctionPenalty = corrections.Select(r => r.PenaltyValue).Sum();

            // Если лицевой счет в статусе "Действующий", то перерасчитываем начисления
            // Не начисляем за будущие периоды
            if ((endDate < DateTime.Now.Date || forceCalcChargeOnFutureDate))
            {
                if (account.IdState == 1)
                {
                    foreach (var tenancy in account.TenancyInfo)
                    {
                        chargeTenancy += CalcChargeByTenancy(tenancy, startDate, endDate);
                    }
                    chargeTenancy = Math.Round(chargeTenancy, 2);
                }

                if (account.IdState == 1 || account.IdState == 3)
                {
                    chargePenalty = Math.Round(CalcPenalty(account.Charges, prevCharges, account.Claims, account.Payments, endDate, startRewriteDate), 2);
                }
            }

            var paymentStartDate = startDate.AddMonths(-1).AddDays(24);
            var paymentEndDate = endDate.AddDays(1).AddMonths(-1).AddDays(23);
            var payments = account.Payments.Where(r =>
                r.DateExecute != null ? r.DateExecute >= paymentStartDate && r.DateExecute <= paymentEndDate :
                r.DateIn != null ? r.DateIn >= paymentStartDate && r.DateIn <= paymentEndDate :
                r.DateDocument != null ? r.DateDocument >= paymentStartDate && r.DateDocument <= paymentEndDate : false);

            var claimIds = account.Claims.Select(r => r.IdClaim).ToList();
            var chargeIds = account.Charges.Select(r => r.IdCharge).ToList();

            var paymentPenalty = payments.Where(r => r.PaymentCharges.Any()).Sum(r => r.PaymentCharges.Where(pc => chargeIds.Contains(pc.IdCharge)).Sum(pc => pc.PenaltyValue))
                + payments.Where(r => r.PaymentClaims.Any()).Sum(r => r.PaymentClaims.Where(pc => claimIds.Contains(pc.IdClaim)).Select(pc => pc.PenaltyValue).Sum());

            var paymentTenancy = payments.Where(r => r.PaymentCharges.Any()).Sum(r => r.PaymentCharges.Where(pc => chargeIds.Contains(pc.IdCharge)).Sum(pc => pc.TenancyValue))
                + payments.Where(r => r.PaymentClaims.Any()).Sum(r => r.PaymentClaims.Where(pc => claimIds.Contains(pc.IdClaim)).Select(pc => pc.TenancyValue).Sum());

            if (chargeTenancy != 0 || chargePenalty != 0 || recalcTenancy != 0 || recalcPenalty != 0 || paymentPenalty != 0 || paymentTenancy != 0 ||
                startDate == DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day+1))
                outOfBound = false;
            else
                outOfBound = true;
            return new KumiCharge
            {
                StartDate = startDate,
                EndDate = endDate,
                IdAccount = account.IdAccount,
                InputTenancy = inputBalanceTenancy,
                InputPenalty = inputBalancePenalty,
                ChargeTenancy = chargeTenancy,
                ChargePenalty = chargePenalty,
                RecalcTenancy = recalcTenancy,
                RecalcPenalty = recalcPenalty,
                CorrectionTenancy = correctionTenancy,
                CorrectionPenalty = correctionPenalty,
                PaymentTenancy = paymentTenancy,
                PaymentPenalty = paymentPenalty,
                OutputTenancy = inputBalanceTenancy + chargeTenancy + recalcTenancy + correctionTenancy - paymentTenancy,
                OutputPenalty = inputBalancePenalty + chargePenalty + recalcPenalty + correctionPenalty - paymentPenalty,
            };
        }

        private decimal CalcPenalty(List<KumiCharge> dbCharges, List<KumiCharge> charges, List<Claim> claims, 
            List<KumiPayment> payments, DateTime? endDate, DateTime startRewriteDate)
        {
            var chargeIds = dbCharges.Select(r => r.IdCharge).ToList();

            var paymentEndDate = endDate.Value.AddDays(1).AddMonths(-1).AddDays(23);

            var calcPayments = payments.Where(r =>
                r.DateExecute != null ? r.DateExecute <= paymentEndDate :
                r.DateIn != null ? r.DateIn <= paymentEndDate :
                r.DateDocument != null ? r.DateDocument <= paymentEndDate : false).Where(r => r.PaymentCharges.Any(pc => chargeIds.Contains(pc.IdCharge)));

            // Для ПИР, которые были до даты последнего начисления БКС:
            // Не учитывать в расчете пени, т.к. оплаты по ним отражены в движении от БКС
            DateTime? bksChargeLastDate = null;
            if (dbCharges.Any(r => r.IsBksCharge == 1))
            {
                bksChargeLastDate = dbCharges.Where(r => r.IsBksCharge == 1).Select(r => r.EndDate).Max();
            }

            var preparedClaims = claims.Where(r =>
                    r.EndDeptPeriod <= paymentEndDate && (bksChargeLastDate == null || r.AtDate > bksChargeLastDate) &&
                    r.ClaimStates.Any(s => s.IdStateType == 4 && s.CourtOrderDate != null) &&
                    !r.ClaimStates.Any(s => s.IdStateType == 6 && s.CourtOrderCancelDate != null))
                .Select(r => new KumiSumDateInfo
                {
                    Date = r.EndDeptPeriod.Value, //r.ClaimStates.FirstOrDefault(s => s.IdStateType == 4).CourtOrderDate.Value,
                    Value = (r.AmountTenancy + r.AmountPkk + r.AmountPadun + r.AmountDgi) ?? 0
                });

            var preparedPayments = calcPayments.Select(r => new KumiSumDateInfo
            {
                Date = (r.DateExecute ?? r.DateIn ?? r.DateDocument).Value,
                Value = r.PaymentCharges.Where(pc => chargeIds.Contains(pc.IdCharge)).Sum(pc=> pc.TenancyValue)
            });

            var resultPayments = preparedClaims.Union(preparedPayments).ToList();

            var resultCharges =
                dbCharges.Where(r => r.StartDate < startRewriteDate && r.EndDate <= endDate)
                .Union(charges.Where(r => r.StartDate >= startRewriteDate && r.EndDate <= endDate));
            var firstChargeInputInfo = resultCharges.OrderBy(r => r.EndDate).FirstOrDefault();
            var resultChargesInfo = resultCharges
                .Select(r => new KumiSumDateInfo
                {
                    Date = r.EndDate,
                    Value = firstChargeInputInfo != null && firstChargeInputInfo.EndDate == r.EndDate ? 
                        r.ChargeTenancy + r.RecalcTenancy + firstChargeInputInfo.InputTenancy : r.ChargeTenancy + r.RecalcTenancy
                }).ToList();

            var penalty = 0m;

            // Расчет пени
            while(resultChargesInfo.Where(r => r.Value > 0).Any() && resultPayments.Where(r => r.Value > 0).Any())
            {
                var firstCharge = resultChargesInfo.Where(r => r.Value > 0).OrderBy(r => r.Date).First();
                var dbCharge = dbCharges.Where(r => r.EndDate == firstCharge.Date).FirstOrDefault();
                while (firstCharge.Value > 0 && resultPayments.Where(r => r.Value > 0).Any())
                {
                    var firstPayment = resultPayments.Where(r => r.Value > 0).OrderBy(r => r.Date).First();
                    var calcSum = 0m;
                    if (firstCharge.Value >= firstPayment.Value)
                    {
                        firstCharge.Value -= firstPayment.Value;
                        calcSum = firstPayment.Value;
                        firstPayment.Value = 0;
                    } else
                    {
                        firstPayment.Value -= firstCharge.Value;
                        calcSum = firstCharge.Value;
                        firstCharge.Value = 0;
                    }
                    if (dbCharge == null || dbCharge.IsBksCharge != 1) // Не считать пени на начисления БКС
                        penalty += CalcPenalty(firstCharge.Date, firstPayment.Date, calcSum, out List<KumiActPeniCalcEventVM> peniCalcEvents);
                }
            }

            foreach(var charge in resultChargesInfo.Where(r => r.Value > 0))
            {
                var dbCharge = dbCharges.Where(r => r.EndDate == charge.Date).FirstOrDefault();
                if (dbCharge == null || dbCharge.IsBksCharge != 1)  // Не считать пени на начисления БКС
                    penalty += CalcPenalty(charge.Date, endDate.Value, charge.Value, out List<KumiActPeniCalcEventVM> peniCalcEvents);
            }

            // Учет итогового пени за все периоды (включая нерасчетного пени от БКС) для последующего вычитания предыдущих периодов
            penalty += charges.Where(r => r.IsBksCharge == 1).Select(r => r.ChargePenalty + r.RecalcPenalty).Sum();
            var prevPenalty = charges.Where(r => r.EndDate < endDate).Sum(r => r.ChargePenalty + r.RecalcPenalty);
            return penalty - prevPenalty;
        }

        private decimal CalcPenalty(DateTime chargeDate, DateTime paymentDate, decimal sum, out List<KumiActPeniCalcEventVM> peniCalcEvents)
        {
            peniCalcEvents = new List<KumiActPeniCalcEventVM>();
            var endPaymentDate = chargeDate.AddDays(10);
            var startPenalty300Date = endPaymentDate.AddDays(31);
            var startPenalty130Date = endPaymentDate.AddDays(91);
            var penaltyCalcInfo = new List<KumiPenaltyCalcInfo>();
            if (paymentDate >= startPenalty300Date)
            {
                penaltyCalcInfo.Add(new KumiPenaltyCalcInfo
                {
                    StartDate = startPenalty300Date,
                    EndDate = paymentDate >= startPenalty130Date ? startPenalty130Date.AddDays(-1) : paymentDate,
                    KeyRate = 0,
                    KeyRateCoef = 1/300m,
                    Sum = sum
                });
            }
            if (paymentDate >= startPenalty130Date)
            {
                penaltyCalcInfo.Add(new KumiPenaltyCalcInfo
                {
                    StartDate = startPenalty130Date,
                    EndDate = paymentDate,
                    KeyRate = 0,
                    KeyRateCoef = 1 / 130m,
                    Sum = sum
                });
            }

            penaltyCalcInfo = UpdatePenaltyCalcInfoKeyRate(penaltyCalcInfo);
            var penalty = 0m;

            foreach(var penaltyInfo in penaltyCalcInfo)
            {
                var currentPenalty = ((penaltyInfo.EndDate - penaltyInfo.StartDate).Days + 1) * (penaltyInfo.KeyRate / 100m) * penaltyInfo.KeyRateCoef * penaltyInfo.Sum;
                peniCalcEvents.Add(new KumiActPeniCalcEventVM {
                    StartDate = penaltyInfo.StartDate,
                    EndDate = penaltyInfo.EndDate,
                    KeyRate = penaltyInfo.KeyRate,
                    KeyRateCoef = penaltyInfo.KeyRateCoef,
                    Tenancy = penaltyInfo.Sum,
                    Penalty = currentPenalty
                });
                penalty += currentPenalty;
            }

            return penalty;
        }

        private List<KumiPenaltyCalcInfo> UpdatePenaltyCalcInfoKeyRate(List<KumiPenaltyCalcInfo> penaltyCalcInfo)
        {
            var result = new List<KumiPenaltyCalcInfo>();

            foreach(var penaltyInfo in penaltyCalcInfo)
            {
                var prevKeyRate = registryContext.KumiKeyRates.Where(r => r.StartDate <= penaltyInfo.StartDate)
                    .OrderByDescending(r => r.StartDate).FirstOrDefault();
                var inPeriodKeyRates = registryContext.KumiKeyRates.Where(r => r.StartDate > penaltyInfo.StartDate && r.StartDate <= penaltyInfo.EndDate).ToList();
                if (prevKeyRate != null)
                {
                    inPeriodKeyRates.Add(prevKeyRate);
                }
                foreach(var keyRate in inPeriodKeyRates.OrderBy(r => r.StartDate))
                {
                    var nextKeyRate = inPeriodKeyRates.Where(r => r.StartDate > keyRate.StartDate).OrderBy(r => r.StartDate).FirstOrDefault();
                    result.Add(new KumiPenaltyCalcInfo
                    {
                        StartDate = penaltyInfo.StartDate >= keyRate.StartDate ? penaltyInfo.StartDate : keyRate.StartDate,
                        EndDate = nextKeyRate != null ? nextKeyRate.StartDate.AddDays(-1) : penaltyInfo.EndDate,
                        KeyRate = keyRate.Value,
                        KeyRateCoef = penaltyInfo.KeyRateCoef,
                        Sum = penaltyInfo.Sum
                    });
                }
            }

            return result;
        }

        private decimal CalcChargeByTenancy(KumiTenancyInfoForPaymentCalculator tenancy, DateTime startDate, DateTime endDate)
        {
            var rentPayment = 0m;
            if (tenancy.AnnualDate != null)
            {
                if (tenancy.AnnualDate <= startDate)
                {
                    return rentPayment;
                }
                if (tenancy.AnnualDate < endDate)
                {
                    endDate = tenancy.AnnualDate.Value;
                }
            }
            foreach(var rentPaymentInfo in tenancy.RentPayments.GroupBy(r => new { r.IdObject, r.AddressType }))
            {
                var actualPaymentPeriods = rentPaymentInfo.Where(r => r.FromDate <= endDate && r.FromDate >= startDate).ToList();
                var prevStartPaymentPeriod = rentPaymentInfo.Where(r => r.FromDate < startDate).OrderByDescending(r => r.FromDate).FirstOrDefault();
                if (prevStartPaymentPeriod != null)
                {
                    actualPaymentPeriods.Add(prevStartPaymentPeriod);
                }
                var paymentPeriods = new List<RentSubPaymentForPaymentCalculator>();
                foreach(var actualPaymentPeriod in actualPaymentPeriods)
                {
                    var paymentPeriod = new RentSubPaymentForPaymentCalculator
                    {
                        PaymentMonth = actualPaymentPeriod.Payment,
                        FromDate = actualPaymentPeriod.FromDate > startDate ? actualPaymentPeriod.FromDate.Date : startDate
                    };
                    var nextPeriod = actualPaymentPeriods.OrderBy(r => r.FromDate).FirstOrDefault(r => r.FromDate > actualPaymentPeriod.FromDate);
                    if (nextPeriod != null && nextPeriod.FromDate.Date == actualPaymentPeriod.FromDate.Date) continue;
                    paymentPeriod.ToDate = nextPeriod == null ? endDate : nextPeriod.FromDate > endDate ? endDate : nextPeriod.FromDate.Date.AddDays(-1);
                    if (paymentPeriod.ToDate < paymentPeriod.FromDate) continue;
                    paymentPeriods.Add(paymentPeriod);
                }

                var actualRentPeriods = tenancy.RentPeriods.Where(r => 
                    (r.FromDate <= startDate && r.ToDate >= startDate) ||
                    (r.FromDate <= endDate && r.ToDate >= endDate) ||
                    (r.FromDate >= startDate && r.ToDate <= endDate)).ToList();
                if (!actualRentPeriods.Any())
                    continue;
                foreach (var rentPeriod in actualRentPeriods)
                {
                    var rentFromDate = rentPeriod.FromDate > startDate ? rentPeriod.FromDate : startDate;
                    var rentToDate = rentPeriod.ToDate > endDate ? endDate : rentPeriod.ToDate;

                    foreach (var paymentPeriod in paymentPeriods)
                    {
                        var from = paymentPeriod.FromDate;
                        var to = paymentPeriod.ToDate;
                        if (paymentPeriod.FromDate <= rentFromDate && paymentPeriod.ToDate >= rentFromDate) // Пересечение левой границы
                        {
                            from = rentFromDate.Value;
                            to = paymentPeriod.ToDate > rentToDate ? rentToDate.Value : paymentPeriod.ToDate;
                        } else
                        if (paymentPeriod.FromDate <= rentToDate && paymentPeriod.ToDate >= rentToDate) // Пересечение правой границы
                        {
                            from = paymentPeriod.FromDate;
                            to = rentToDate.Value;
                        } else
                        if (paymentPeriod.FromDate >= rentFromDate && paymentPeriod.ToDate <= rentToDate) // Вложенный период
                        {
                            from = paymentPeriod.FromDate;
                            to = paymentPeriod.ToDate;
                        }
                        var diffDays = (to - from).Days + 1;
                        var monthDays = DateTime.DaysInMonth(from.Year, from.Month);
                        var payment = paymentPeriod.PaymentMonth / monthDays * diffDays;
                        rentPayment += payment;
                    }
                }
            }
            return rentPayment;
        }

        public List<KumiAccountPrepareForPaymentCalculator> GetAccountsPrepareForPaymentCalculator(IQueryable<KumiAccount> accounts)
        {
            var result = new List<KumiAccountPrepareForPaymentCalculator>();
            var tenancyInfo = GetTenancyInfo(accounts);
            foreach(var account in accounts.Include(r => r.Charges).Include(r => r.Claims).Include(r => r.Corrections).AsNoTracking().ToList())
            {
                var accountInfo = new KumiAccountPrepareForPaymentCalculator();
                accountInfo.Account = account;
                if (tenancyInfo.ContainsKey(account.IdAccount))
                {
                    accountInfo.TenancyInfo = tenancyInfo[account.IdAccount];
                    accountInfo.TenancyPaymentHistories = new Dictionary<int, List<TenancyPaymentHistory>>();
                    foreach (var tenancy in accountInfo.TenancyInfo)
                    {
                        var paymentsHistory = new List<TenancyPaymentHistory>();
                        foreach(var rentObject in tenancy.RentObjects)
                        {
                            if (!int.TryParse(rentObject.Address.Id, out int id)) continue;
                            switch (rentObject.Address.AddressType)
                            {
                                case AddressTypes.Building:
                                    paymentsHistory.AddRange(registryContext.TenancyPaymentsHistory.Where(r => r.IdBuilding == id
                                        && r.IdPremises == null));
                                    break;
                                case AddressTypes.Premise:
                                    paymentsHistory.AddRange(registryContext.TenancyPaymentsHistory.Where(r => r.IdPremises == id
                                        && r.IdSubPremises == null));
                                    break;
                                case AddressTypes.SubPremise:
                                    paymentsHistory.AddRange(registryContext.TenancyPaymentsHistory.Where(r => r.IdSubPremises == id));
                                    break;
                            }
                        }
                        if (!accountInfo.TenancyPaymentHistories.ContainsKey(tenancy.TenancyProcess.IdProcess))
                            accountInfo.TenancyPaymentHistories.Add(tenancy.TenancyProcess.IdProcess, paymentsHistory);
                    }
                }
                var chargesIds = account.Charges.Select(r => r.IdCharge).ToList();
                var claimsIds = account.Claims.Select(r => r.IdClaim).ToList();

                // Берем только платежи после даты последнего начисления БКС
                accountInfo.Payments = registryContext.KumiPayments.Include(p => p.PaymentCharges)
                    .Where(p => p.PaymentCharges.Any(r => chargesIds.Contains(r.IdCharge))).ToList()
                    .Union(
                        registryContext.KumiPayments.Include(p => p.PaymentClaims)
                        .Where(p => p.PaymentClaims.Any(r => claimsIds.Contains(r.IdClaim))).ToList())
                    .ToList();

                DateTime? maxBksCharegeDate = null;
                if (account.Charges.Any(r => r.IsBksCharge == 1))
                {
                    maxBksCharegeDate = account.Charges.Where(r => r.IsBksCharge == 1).Select(r => r.EndDate).Max();
                    maxBksCharegeDate = maxBksCharegeDate.Value.AddDays(-maxBksCharegeDate.Value.Day+1).AddDays(24); // 25 число месяца, когда было начисление БКС
                }

                accountInfo.Payments = accountInfo.Payments.Where(r => maxBksCharegeDate == null || (r.DateExecute ?? r.DateIn ?? r.DateDocument).Value > maxBksCharegeDate).ToList();

                // Платежи по начислениям БКС (замороженные строки без фактических платежей)
                var bksPayments = registryContext.KumiCharges.Where(r => r.IdAccount == account.IdAccount && r.IsBksCharge == 1 && r.PaymentTenancy + r.PaymentPenalty > 0)
                    .Select(r => new KumiPayment {
                        DateIn = r.StartDate,
                        DateExecute = r.StartDate,
                        DateDocument = r.StartDate,
                        Sum = r.PaymentTenancy+r.PaymentPenalty,
                        PaymentCharges = new List<KumiPaymentCharge> {
                            new KumiPaymentCharge {
                                Date = r.StartDate,
                                TenancyValue = r.PaymentTenancy,
                                IdCharge = r.IdCharge,
                                IdDisplayCharge = r.IdCharge,
                            },
                            new KumiPaymentCharge {
                                Date = r.StartDate,
                                PenaltyValue = r.PaymentPenalty,
                                IdCharge = r.IdCharge,
                                IdDisplayCharge = r.IdCharge,
                            },
                        }
                    });

                accountInfo.Payments.AddRange(bksPayments);

                // Исковые работы
                accountInfo.Claims = account.Claims.ToList();
                foreach(var claim in accountInfo.Claims)
                {
                    claim.ClaimStates = registryContext.ClaimStates.Where(r => r.IdClaim == claim.IdClaim).ToList();
                }
                if (accountInfo.TenancyInfo == null)
                    accountInfo.TenancyInfo = new List<KumiAccountTenancyInfoVM>();
                result.Add(accountInfo);
            }
            return result;
        }

        public List<KumiAccountInfoForPaymentCalculator> GetAccountInfoForPaymentCalculator(List<KumiAccountPrepareForPaymentCalculator> accounts)
        {
            var result = new List<KumiAccountInfoForPaymentCalculator>();
            foreach(var account in accounts)
            {
                var accountInfo = new KumiAccountInfoForPaymentCalculator
                {
                    IdAccount = account.Account.IdAccount,
                    IdState = account.Account.IdState,
                    Account = account.Account.Account,
                    LastChargeDate = account.Account.LastChargeDate,
                    CurrentBalanceTenancy = account.Account.CurrentBalanceTenancy ?? 0,
                    CurrentBalancePenalty = account.Account.CurrentBalancePenalty ?? 0
                };
                accountInfo.Payments = account.Payments;
                accountInfo.Claims = account.Claims;
                accountInfo.Charges = account.Account.Charges.ToList();
                accountInfo.Corrections = account.Account.Corrections.ToList();

                accountInfo.TenancyInfo = new List<KumiTenancyInfoForPaymentCalculator>();
                foreach(var tenancy in account.TenancyInfo)
                {
                    var tenancyInfo = new KumiTenancyInfoForPaymentCalculator
                    {
                        IdProcess = tenancy.TenancyProcess.IdProcess,
                        RegistrationDate = tenancy.TenancyProcess.RegistrationDate,
                        AnnualDate = tenancy.TenancyProcess.AnnualDate,
                        RegistrationNum = tenancy.TenancyProcess.RegistrationNum,
                        Tenant = tenancy.Tenant
                    };
                    tenancyInfo.RentPeriods = BuildRentPeriodsForPaymentCalculator(tenancy.TenancyProcess);
                    tenancyInfo.RentPeriods = JoinRentPeriodForPaymentCalculator(tenancyInfo.RentPeriods);
                    CorrectRentPeriodsForPaymentCalculator(tenancyInfo.RentPeriods, tenancy.TenancyProcess, account.Account.CreateDate);
                    CheckRentPeriodsForPaymentCalculator(tenancyInfo.RentPeriods, tenancy.TenancyProcess.IdProcess);
                    tenancyInfo.RentPeriods = JoinRentPeriodForPaymentCalculator(tenancyInfo.RentPeriods);
                    if (tenancyInfo.RentPeriods.Any())
                        accountInfo.TenancyInfo.Add(tenancyInfo);

                    tenancyInfo.RentPayments = BuldRentPaymentsForPaymentCalculator(account.TenancyPaymentHistories, 
                        tenancyInfo.RentPeriods, tenancy);
                }
                result.Add(accountInfo);
            }
            return result;
        }

        private List<RentPaymentForPaymentCalculator> BuldRentPaymentsForPaymentCalculator(Dictionary<int, List<TenancyPaymentHistory>> tenancyPaymentHistories, 
            List<RentPeriodForPaymentCalculator> rentPeriods, KumiAccountTenancyInfoVM tenancyInfo)
        {
            var result = new List<RentPaymentForPaymentCalculator>();
            if (tenancyPaymentHistories.ContainsKey(tenancyInfo.TenancyProcess.IdProcess))
            {
                foreach (var historyInfo in tenancyPaymentHistories[tenancyInfo.TenancyProcess.IdProcess])
                {
                    var rentArea = historyInfo.RentArea;
                    var payment = new RentPaymentForPaymentCalculator();
                    if (historyInfo.IdSubPremises != null)
                    {
                        payment.AddressType = AddressTypes.SubPremise;
                        payment.IdObject = historyInfo.IdSubPremises.Value;
                        var rentObject = 
                            tenancyInfo.RentObjects.FirstOrDefault(r => r.Address.AddressType == AddressTypes.SubPremise && r.Address.Id == payment.IdObject.ToString());
                        if (rentObject != null && rentObject.RentArea != null)
                            rentArea = rentObject.RentArea.Value;
                    }
                    else
                    if (historyInfo.IdPremises != null)
                    {
                        payment.AddressType = AddressTypes.Premise;
                        payment.IdObject = historyInfo.IdPremises.Value;
                        var rentObject =
                             tenancyInfo.RentObjects.FirstOrDefault(r => r.Address.AddressType == AddressTypes.Premise && r.Address.Id == payment.IdObject.ToString());
                        if (rentObject != null && rentObject.RentArea != null)
                            rentArea = rentObject.RentArea.Value;
                    }
                    else
                    {
                        payment.AddressType = AddressTypes.Building;
                        payment.IdObject = historyInfo.IdBuilding;
                        var rentObject =
                            tenancyInfo.RentObjects.FirstOrDefault(r => r.Address.AddressType == AddressTypes.Building && r.Address.Id == payment.IdObject.ToString());
                        if (rentObject != null && rentObject.RentArea != null)
                            rentArea = rentObject.RentArea.Value;
                    }
                    payment.FromDate = historyInfo.Date;

                    
                    payment.Payment = Math.Round((historyInfo.K1 + historyInfo.K2 + historyInfo.K3) / 3
                        * historyInfo.Kc * (decimal)rentArea * historyInfo.Hb * tenancyInfo.AccountAssoc.Fraction, 2);
                    result.Add(payment);
                }
            }

            if (!result.Any())
                foreach (var rentObject in tenancyInfo.RentObjects)
                {
                    var payment = new RentPaymentForPaymentCalculator
                    {
                        AddressType = rentObject.Address.AddressType
                    };
                    if (int.TryParse(rentObject.Address.Id, out int id))
                        payment.IdObject = id;
                    else continue;
                    var firstRentPeriod = rentPeriods.OrderBy(r => r.FromDate).ThenBy(r => r.ToDate).FirstOrDefault();
                    if (firstRentPeriod != null)
                        payment.FromDate = firstRentPeriod.FromDate.Value;
                    else
                        payment.FromDate = DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day+1).AddMonths(-1);
                    payment.Payment = rentObject.Payment * tenancyInfo.AccountAssoc.Fraction;
                    result.Add(payment);
                }
            return result;
        }

        private List<RentPeriodForPaymentCalculator> BuildRentPeriodsForPaymentCalculator(TenancyProcess process)
        {
            var result = new List<RentPeriodForPaymentCalculator>();
            var rentPeriod = new RentPeriodForPaymentCalculator();

            if (process.EndDate != null)
                rentPeriod.ToDate = process.EndDate.Value;
            else
                rentPeriod.ToDate = DateTime.MaxValue;

            if (process.BeginDate != null)
                rentPeriod.FromDate = process.BeginDate.Value;

            result.Add(rentPeriod);

            for (var i = 0; i < process.TenancyRentPeriods.Count; i++)
            {
                var subRentPeriod = process.TenancyRentPeriods.ToList()[i];
                rentPeriod = new RentPeriodForPaymentCalculator();

                if (subRentPeriod.BeginDate != null)
                    rentPeriod.FromDate = subRentPeriod.BeginDate.Value;
                if (subRentPeriod.EndDate != null)
                    rentPeriod.ToDate = subRentPeriod.EndDate.Value;

                if (rentPeriod.ToDate != null || rentPeriod.FromDate != null)
                    result.Add(rentPeriod);
            }
            return result;
        }

        private void CheckRentPeriodsForPaymentCalculator(List<RentPeriodForPaymentCalculator> rentPeriods, int idProcess)
        {
            for (var i = 0; i < rentPeriods.Count; i++)
            {
                var currentPeriod = rentPeriods[i];
                if (currentPeriod.FromDate == null || currentPeriod.ToDate == null)
                    throw new ApplicationException(string.Format("Ошибка определения периодов найма № {0}", idProcess));
            }
        }

        private void CorrectRentPeriodsForPaymentCalculator(List<RentPeriodForPaymentCalculator> rentPeriods, TenancyProcess process, DateTime accountCreateDate)
        {
            for (var i = 0; i < rentPeriods.Count; i++)
            {
                var currentPeriod = rentPeriods[i];
                RentPeriodForPaymentCalculator prevPeriod = rentPeriods
                                .Where(r => r != currentPeriod)
                                .Where(r => 
                                    r.ToDate <= currentPeriod.FromDate || (r.ToDate == null && r.FromDate <= currentPeriod.FromDate) || 
                                    r.ToDate <= currentPeriod.ToDate || (r.ToDate == null && r.FromDate <= currentPeriod.ToDate))
                                .OrderByDescending(r => r.ToDate == null ? r.FromDate : r.ToDate)
                                .FirstOrDefault();
                RentPeriodForPaymentCalculator nextPeriod = rentPeriods
                                .Where(r => r != currentPeriod)
                                .Where(r => 
                                    r.FromDate >= currentPeriod.ToDate || (r.FromDate == null && r.ToDate >= currentPeriod.ToDate) || 
                                    r.FromDate >= currentPeriod.FromDate || (r.FromDate == null && r.FromDate >= currentPeriod.FromDate))
                                .OrderBy(r => r.FromDate == null ? r.ToDate : r.FromDate)
                                .FirstOrDefault();

                if (nextPeriod == null && currentPeriod.ToDate == null)
                {
                    currentPeriod.ToDate = DateTime.MaxValue;
                }
                if (prevPeriod == null && currentPeriod.FromDate == null)
                {
                    if (process.RegistrationDate != null && currentPeriod.ToDate >= process.RegistrationDate.Value)
                    {
                        currentPeriod.FromDate = process.RegistrationDate.Value;
                    }
                    else
                    if (process.IssueDate != null && currentPeriod.ToDate >= process.IssueDate.Value)
                    {
                        currentPeriod.FromDate = process.IssueDate.Value;
                    }
                }
                if (currentPeriod.FromDate == null && prevPeriod != null && prevPeriod.ToDate != null)
                {
                    currentPeriod.FromDate = prevPeriod.ToDate.Value == DateTime.MaxValue ? prevPeriod.ToDate : prevPeriod.ToDate.Value.AddDays(1);
                }
                if (currentPeriod.FromDate != null && prevPeriod != null && prevPeriod.ToDate == null)
                {
                    prevPeriod.ToDate = currentPeriod.FromDate.Value.AddDays(-1);
                }
                if (currentPeriod.ToDate == null && nextPeriod != null && nextPeriod.FromDate == null)
                {
                    currentPeriod.ToDate = nextPeriod.ToDate;
                    nextPeriod.FromDate = currentPeriod.FromDate;
                }
                if (currentPeriod.FromDate == null && prevPeriod != null && prevPeriod.ToDate == null)
                {
                    currentPeriod.FromDate = prevPeriod.FromDate;
                    prevPeriod.ToDate = currentPeriod.ToDate;
                }
                if (prevPeriod == null && nextPeriod == null && currentPeriod.FromDate == null)
                {
                    currentPeriod.FromDate = accountCreateDate;
                }
            }
        }

        private List<RentPeriodForPaymentCalculator> JoinRentPeriodForPaymentCalculator(List<RentPeriodForPaymentCalculator> rentPeriods)
        {
            var result = new List<RentPeriodForPaymentCalculator>();
            var skipPeriods = new List<RentPeriodForPaymentCalculator>();

            foreach (var rentPeriod in rentPeriods.OrderByDescending(r => r.FromDate))
            {
                var currentPeriod = rentPeriod;
                if (skipPeriods.Any(r => r == currentPeriod)) continue;
                while (true)
                {
                    var joinWithPeriodsTo = rentPeriods.Except(skipPeriods).Where(r => r != currentPeriod && r.ToDate != null && r.FromDate != null)
                        .ToList().Where(r => (r.ToDate.Value == DateTime.MaxValue ? r.ToDate : r.ToDate.Value.AddDays(1)) >= currentPeriod.FromDate && r.FromDate < currentPeriod.ToDate).ToList();
                    var joinWithPeriodsFrom = rentPeriods.Except(skipPeriods).Where(r => r != currentPeriod && r.FromDate != null && r.ToDate != null)
                        .ToList().Where(r => r.FromDate.Value.AddDays(-1) <= currentPeriod.ToDate && r.ToDate >= currentPeriod.FromDate).ToList();

                    if (!joinWithPeriodsTo.Any() && !joinWithPeriodsFrom.Any())
                        break;

                    var maxToDate = joinWithPeriodsFrom.Max(r => r.ToDate);
                    var minFromDate = joinWithPeriodsTo.Min(r => r.FromDate);
                    if (currentPeriod.ToDate < maxToDate)
                    {
                        currentPeriod.ToDate = maxToDate;
                    }
                    if (currentPeriod.FromDate > minFromDate)
                    {
                        currentPeriod.FromDate = minFromDate;
                    }
                    skipPeriods.AddRange(joinWithPeriodsTo);
                    skipPeriods.AddRange(joinWithPeriodsFrom);
                }
                skipPeriods.Add(currentPeriod);
                result.Add(currentPeriod);
            }
            return result;
        }

        public KumiAccountsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            KumiAccountsFilter filterOptions, out List<int> filteredIds)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var accounts = GetQuery();
            viewModel.PageOptions.TotalRows = accounts.Count();
            var query = GetQueryFilter(accounts, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            filteredIds = query.Select(c => c.IdAccount).ToList();

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.Accounts = query.ToList();
            viewModel.TenancyInfo = GetTenancyInfo(viewModel.Accounts);
            viewModel.ClaimsInfo = GetClaimsInfo(viewModel.Accounts);
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "IdRegion", "Region");
            viewModel.KladrStreetsList = new SelectList(addressesDataService.GetKladrStreets(filterOptions?.IdRegion), "IdStreet", "StreetName");

            return viewModel;
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

            accounts = GetQueryPage(accounts, viewModel.PageOptions);
            viewModel.Accounts = accounts.ToList();
            viewModel.TenancyInfo = GetTenancyInfo(viewModel.Accounts);
            viewModel.ClaimsInfo = GetClaimsInfo(viewModel.Accounts);

            return viewModel;
        }

        public IQueryable<KumiAccount> GetAccountsForMassReports(List<int> ids)
        {
            return GetQuery().Where(p => ids.Contains(p.IdAccount)).AsNoTracking();
        }

        public Dictionary<int, List<string>> GetTenantsEmails(List<KumiAccount> accounts)
        {
            var emailsDic = new Dictionary<int, List<string>>();
            foreach (var account in accounts)
            {
                var processes = account.AccountsTenancyProcessesAssoc.Select(r => r.IdProcess);

                List<string> emails = new List<string>();
                foreach (var tp in processes)
                {
                    var curEmails = registryContext.TenancyPersons
                        .Where(per => per.IdProcess == tp && per.Email != null)
                        .Select(per => per.Email)
                        .ToList();
                    emails.AddRange(curEmails);
                }
                emails = emails.Distinct().ToList();
                emailsDic.Add(account.IdAccount, emails);
            }
            return emailsDic;
        }

        public IQueryable<KumiAccount> GetKumiAccounts(KumiAccountsFilter filterOptions)
        {
            var kumiAccounts = GetQuery();
            var query = GetQueryFilter(kumiAccounts, filterOptions);
            return query;
        }

        private IQueryable<KumiAccount> GetQueryOrder(IQueryable<KumiAccount> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField))
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdAccount);
                else
                    return query.OrderByDescending(p => p.IdAccount);
            }
            if (orderOptions.OrderField == "Account")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.Account);
                else
                    return query.OrderByDescending(p => p.Account);
            }
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

        private IQueryable<KumiAccount> GetQueryIncludes(IQueryable<KumiAccount> query)
        {
            return query.Include(r => r.AccountsTenancyProcessesAssoc)
                .Include(r => r.Claims)
                .Include(r => r.Charges);
        }

        private IQueryable<KumiAccount> GetQueryFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = AccountFilter(query, filterOptions);
                query = BalanceFilter(query, filterOptions);
                query = ClaimsBehaviorFilter(query, filterOptions);
                query = EmailsFilter(query, filterOptions);
                query = PresetsFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<KumiAccount> AddressFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty() &&
                string.IsNullOrEmpty(filterOptions.IdStreet) &&
                string.IsNullOrEmpty(filterOptions.House) &&
                string.IsNullOrEmpty(filterOptions.PremisesNum) &&
                string.IsNullOrEmpty(filterOptions.IdRegion) &&
                filterOptions.IdBuilding == null &&
                filterOptions.IdPremises == null &&
                filterOptions.IdSubPremises == null)
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            IEnumerable<int> idAccounts = new List<int>();
            var filtered = false;

            if (filterOptions.Address.AddressType == AddressTypes.Street || !string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var streets = filterOptions.Address.AddressType == AddressTypes.Street ? addresses : new List<string> { filterOptions.IdStreet };
                var infixes = streets.Select(r => string.Concat("s", r));

                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                filtered = true;
            } else
            if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                var infix = "s"+filterOptions.IdRegion.ToString();
                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(new List<string> { infix });
                filtered = true;
            }
            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));

            if ((filterOptions.Address.AddressType == AddressTypes.Building && addressesInt.Any()) || filterOptions.IdBuilding != null)
            {
                if (filterOptions.IdBuilding != null)
                {
                    addressesInt = new List<int> { filterOptions.IdBuilding.Value };
                }

                var infixes = from buildingBow in registryContext.Buildings
                              where addressesInt.Contains(buildingBow.IdBuilding)
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString());
                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.Premise && addressesInt.Any()) || filterOptions.IdPremises != null)
            {
                if (filterOptions.IdPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdPremises.Value };
                }
                
                var infixes = from buildingBow in registryContext.Buildings
                                join premisesRow in registryContext.Premises
                                on buildingBow.IdBuilding equals premisesRow.IdBuilding
                              where addressesInt.Contains(premisesRow.IdPremises)
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString(), "p", premisesRow.IdPremises.ToString());
                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.SubPremise && addressesInt.Any()) || filterOptions.IdSubPremises != null)
            {
                if (filterOptions.IdSubPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdSubPremises.Value };
                }

                var infixes = from buildingBow in registryContext.Buildings
                              join premisesRow in registryContext.Premises
                              on buildingBow.IdBuilding equals premisesRow.IdBuilding
                              join subPremisesRow in registryContext.SubPremises
                              on premisesRow.IdPremises equals subPremisesRow.IdPremises
                              where addressesInt.Contains(subPremisesRow.IdSubPremises)
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString(), "p", 
                                premisesRow.IdPremises.ToString(), "sp", subPremisesRow.IdSubPremises.ToString());
                idAccounts = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                filtered = true;
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var infixes = from buildingBow in registryContext.Buildings
                              where buildingBow.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant())
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString());
                var idAccountsBuf = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                if (filtered)
                    idAccounts = idAccounts.Intersect(idAccountsBuf);
                else
                    idAccounts = idAccountsBuf;
                filtered = true;

            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var infixes = from buildingBow in registryContext.Buildings
                              join premisesRow in registryContext.Premises
                              on buildingBow.IdBuilding equals premisesRow.IdBuilding
                              where premisesRow.PremisesNum.ToLower().Equals(filterOptions.PremisesNum.ToLowerInvariant())
                              select string.Concat("s", buildingBow.IdStreet, "b", buildingBow.IdBuilding.ToString(), "p", premisesRow.IdPremises.ToString());
                var idAccountsBuf = registryContext.GetKumiAccountIdsByAddressInfixes(infixes.ToList());
                if (filtered)
                    idAccounts = idAccounts.Intersect(idAccountsBuf);
                else
                    idAccounts = idAccountsBuf;
                filtered = true;
            }
            if (filtered)
            {
                query = from q in query
                        where idAccounts.Contains(q.IdAccount)
                        select q;
            }
            return query;
        }

        private IQueryable<KumiAccount> AccountFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.FrontSideAccount))
            {
                query = query.Where(a => a.Account.Contains(filterOptions.FrontSideAccount));
            }
            if (!string.IsNullOrEmpty(filterOptions.AccountGisZkh))
            {
                query = query.Where(a => a.AccountGisZkh.Contains(filterOptions.AccountGisZkh));
            }
            if (!string.IsNullOrEmpty(filterOptions.Account))
            {
                query = query.Where(a => a.Account.Contains(filterOptions.Account));
            }
            if (filterOptions.IdAccountState != null)
            {
                query = query.Where(a => a.IdState == filterOptions.IdAccountState);
            }
            if (!string.IsNullOrEmpty(filterOptions.Tenant))
            {
                var tenantParts = filterOptions.Tenant.Split(' ', 3);
                var surname = tenantParts[0].ToLowerInvariant();
                var tenancyPersons = registryContext.TenancyPersons.Include(tp => tp.IdProcessNavigation).ThenInclude(tp => tp.AccountsTenancyProcessesAssoc)
                    .Where(tp => tp.IdProcessNavigation.AccountsTenancyProcessesAssoc != null && 
                    tp.IdProcessNavigation.AccountsTenancyProcessesAssoc.Count() > 0 && tp.Surname.Contains(surname));
                if (tenantParts.Length > 1)
                {
                    var name = tenantParts[1].ToLowerInvariant();
                    tenancyPersons = tenancyPersons.Where(tp => tp.Name.Contains(name));
                }
                if (tenantParts.Length > 2)
                {
                    var patronymic = tenantParts[2].ToLowerInvariant();
                    tenancyPersons = tenancyPersons.Where(tp => tp.Patronymic.Contains(patronymic));
                }

                var idAccounts = tenancyPersons.SelectMany(tp => tp.IdProcessNavigation.AccountsTenancyProcessesAssoc.Select(r => r.IdAccount)).Distinct().ToList();

                query = query.Where(a => idAccounts.Contains(a.IdAccount));
            }
            return query;
        }

        private IQueryable<KumiAccount> BalanceFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.CurrentBalanceTenancy != null)
            {
                query = query.Where(p => filterOptions.CurrentBalanceTenancyOp == 1 ?
                        p.CurrentBalanceTenancy >= filterOptions.CurrentBalanceTenancy :
                        p.CurrentBalanceTenancy <= filterOptions.CurrentBalanceTenancy);
            }
            if (filterOptions.CurrentBalancePenalty != null)
            {
                query = query.Where(p => filterOptions.CurrentBalancePenaltyOp == 1 ?
                        p.CurrentBalancePenalty >= filterOptions.CurrentBalancePenalty :
                        p.CurrentBalancePenalty <= filterOptions.CurrentBalancePenalty);
            }
            if (filterOptions.CurrentBalanceTotal != null)
            {
                query = query.Where(p => filterOptions.CurrentBalanceTotalOp == 1 ?
                        (p.CurrentBalancePenalty+p.CurrentBalanceTenancy) >= filterOptions.CurrentBalanceTotal :
                        (p.CurrentBalancePenalty + p.CurrentBalanceTenancy) <= filterOptions.CurrentBalanceTotal);
            }
            return query;
        }

        private IQueryable<KumiAccount> ClaimsBehaviorFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.IdClaimsBehavior != null)
            {
                var filterIdAccounts = registryContext.Claims.Where(r => r.EndedForFilter)
                    .Select(r => r.IdAccountKumi).Distinct().ToList();
                switch (filterOptions.IdClaimsBehavior)
                {
                    case 1:
                        query = query.Where(r => !filterIdAccounts.Contains(r.IdAccount));
                        break;
                    case 2:
                        query = query.Where(r => filterIdAccounts.Contains(r.IdAccount));
                        break;
                }
            }
            return query;
        }

        private IQueryable<KumiAccount> EmailsFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (!filterOptions.Emails)
            {
                return query;
            }
            var idAccounts = registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons).Include(tp => tp.AccountsTenancyProcessesAssoc)
                    .Where(r => r.TenancyPersons.Count(p => p.Email != null) > 0 && r.AccountsTenancyProcessesAssoc.Count() > 0)
                    .SelectMany(r => r.AccountsTenancyProcessesAssoc.Select(atpa => atpa.IdAccount));

            query = query.Where(r => idAccounts.Contains(r.IdAccount));
            return query;
        }

        private IQueryable<KumiAccount> PresetsFilter(IQueryable<KumiAccount> query, KumiAccountsFilter filterOptions)
        {
            if (filterOptions.IdPreset == null) {
                return query;
            }

            switch (filterOptions.IdPreset)
            {
                case 1:
                case 2:
                    var ids = registryContext.KumiAccounts.Include(a => a.Claims)
                        .Where(a => a.Claims.Any()).Select(a => a.IdAccount).ToList();
                    if (filterOptions.IdPreset == 1)
                    {
                        // Лицевые счета без исковых работ
                        query = from row in query
                                where !ids.Contains(row.IdAccount)
                                select row;
                    }
                    else
                    {
                        // Лицевые счета с исковыми работами (включая завершенные)
                        query = from row in query
                                where ids.Contains(row.IdAccount)
                                select row;
                    }
                    break;
                case 3:
                case 4:
                    var claimsInfo = GetClaimsInfo(query.ToList());
                    ids = new List<int>();

                    foreach (var claimInfo in claimsInfo)
                    {
                        if (claimInfo.Value.Any())
                        {
                            foreach (var claimStateInfo in claimInfo.Value)
                            {
                                if (claimStateInfo.IdClaimCurrentState != 6)
                                {
                                    ids.Add(claimInfo.Key);
                                    break;
                                }
                            }
                        }
                    }
                    if (filterOptions.IdPreset == 3)
                    {
                        // Лицевые счета с незавершенными исковыми работами
                        query = from row in query
                                where ids.Contains(row.IdAccount)
                                select row;
                    }
                    else
                    {
                        // Лицевые счета, в которых отсутствуют незавершенные исковые работы
                        query = from row in query
                                where !ids.Contains(row.IdAccount)
                                select row;
                    }
                    break;
                case 5:
                    // Лицевые счета без привязки к найму
                    query = from row in query
                            where !row.AccountsTenancyProcessesAssoc.Any()
                            select row;
                    break;
                case 6:
                case 7:
                    var idProcessWithTenants = (from row in registryContext.TenancyPersons
                                               where row.ExcludeDate == null || row.ExcludeDate > DateTime.Now
                                               select row.IdProcess).Distinct();

                    var actualAccountIds = (from row in registryContext.TenancyProcesses.Include(r => r.AccountsTenancyProcessesAssoc)
                                            join idProcess in idProcessWithTenants
                                            on row.IdProcess equals idProcess
                                            where (row.RegistrationNum == null || !row.RegistrationNum.EndsWith("н")) && row.AccountsTenancyProcessesAssoc.Count() > 0
                                            select row).SelectMany(r => r.AccountsTenancyProcessesAssoc.Select(atpa => atpa.IdAccount)).ToList();
                    if (filterOptions.IdPreset == 6)
                    {
                        // Действующие лицевые счета без действующих наймов
                        query = from row in query
                                where row.AccountsTenancyProcessesAssoc.Any() && row.IdState == 1 && !actualAccountIds.Contains(row.IdAccount)
                                select row;
                    } else
                    {
                        // Аннулированные лицевые счета с действующими наймами
                        query = from row in query
                                where row.AccountsTenancyProcessesAssoc.Any() && row.IdState == 2 && actualAccountIds.Contains(row.IdAccount)
                                select row;
                    }
                    break;
                case 8:
                    query = from row in query
                            where row.RecalcMarker == 1
                            select row;
                    break;
            }
            return query;
        }

        private IQueryable<KumiAccount> GetQueryPage(IQueryable<KumiAccount> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        public Dictionary<int, List<KumiAccountTenancyInfoVM>> GetTenancyInfo(IEnumerable<KumiAccount> accounts)
        {
            var accountIds = (from account in accounts
                              select
                                  account.IdAccount).ToList();
            var accountTenancyAssocs = (from assoc in registryContext.KumiAccountsTenancyProcessesAssocs
                                       where accountIds.Contains(assoc.IdAccount)
                                       select assoc).ToList();
            var tenancyIds = accountTenancyAssocs.Select(r => r.IdProcess).Distinct();
            var tenancyProcesses = registryContext.TenancyProcesses.Include(r => r.TenancyRentPeriods)
                .Include(r => r.TenancyPersons).Where(r => tenancyIds.Contains(r.IdProcess)).ToList();

            var buildings = from tbaRow in registryContext.TenancyBuildingsAssoc
                            join buildingRow in registryContext.Buildings.Include(r => r.IdStateNavigation)
                            on tbaRow.IdBuilding equals buildingRow.IdBuilding
                            join streetRow in registryContext.KladrStreets
                            on buildingRow.IdStreet equals streetRow.IdStreet
                            where tenancyIds.Contains(tbaRow.IdProcess)
                            select new
                            {
                                tbaRow.IdProcess,
                                RentObject = new TenancyRentObject
                                {
                                    Address = new Address
                                    {
                                        AddressType = AddressTypes.Building,
                                        Id = buildingRow.IdBuilding.ToString(),
                                        ObjectState = buildingRow.IdStateNavigation,
                                        IdParents = new Dictionary<string, string> {
                                            { AddressTypes.Street.ToString(), buildingRow.IdStreet }
                                        },
                                        Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House)
                                    },
                                    TotalArea = buildingRow.TotalArea,
                                    LivingArea = buildingRow.LivingArea,
                                    RentArea = tbaRow.RentTotalArea
                                }
                            };
            var premises = from tpaRow in registryContext.TenancyPremisesAssoc
                           join premiseRow in registryContext.Premises.Include(r => r.IdStateNavigation)
                           on tpaRow.IdPremise equals premiseRow.IdPremises
                           join buildingRow in registryContext.Buildings
                           on premiseRow.IdBuilding equals buildingRow.IdBuilding
                           join streetRow in registryContext.KladrStreets
                           on buildingRow.IdStreet equals streetRow.IdStreet
                           join premiseTypesRow in registryContext.PremisesTypes
                           on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                           where tenancyIds.Contains(tpaRow.IdProcess)
                           select new
                           {
                               tpaRow.IdProcess,
                               RentObject = new TenancyRentObject
                               {
                                   Address = new Address
                                   {
                                       AddressType = AddressTypes.Premise,
                                       Id = premiseRow.IdPremises.ToString(),
                                       ObjectState = premiseRow.IdStateNavigation,
                                       IdParents = new Dictionary<string, string>
                                       {
                                           { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                           { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() }
                                       },
                                       Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                        premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum)
                                   },
                                   TotalArea = premiseRow.TotalArea,
                                   LivingArea = premiseRow.LivingArea,
                                   RentArea = tpaRow.RentTotalArea
                               }
                           };
            var subPremises = from tspaRow in registryContext.TenancySubPremisesAssoc
                              join subPremiseRow in registryContext.SubPremises.Include(r => r.IdStateNavigation)
                              on tspaRow.IdSubPremise equals subPremiseRow.IdSubPremises
                              join premiseRow in registryContext.Premises
                              on subPremiseRow.IdPremises equals premiseRow.IdPremises
                              join buildingRow in registryContext.Buildings
                              on premiseRow.IdBuilding equals buildingRow.IdBuilding
                              join streetRow in registryContext.KladrStreets
                              on buildingRow.IdStreet equals streetRow.IdStreet
                              join premiseTypesRow in registryContext.PremisesTypes
                              on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                              where tenancyIds.Contains(tspaRow.IdProcess)
                              select new
                              {
                                  tspaRow.IdProcess,
                                  RentObject = new TenancyRentObject
                                  {
                                      Address = new Address
                                      {
                                          AddressType = AddressTypes.SubPremise,
                                          Id = subPremiseRow.IdSubPremises.ToString(),
                                          ObjectState = subPremiseRow.IdStateNavigation,
                                          IdParents = new Dictionary<string, string>
                                           {
                                              { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                              { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() },
                                              { AddressTypes.Premise.ToString(), premiseRow.IdPremises.ToString() }
                                           },
                                          Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                            premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum, ", к.", subPremiseRow.SubPremisesNum)
                                      },
                                      TotalArea = subPremiseRow.TotalArea,
                                      LivingArea = subPremiseRow.LivingArea,
                                      RentArea = tspaRow.RentTotalArea
                                  }
                              };

            var objects = buildings.Union(premises).Union(subPremises).ToList();

            var payments = (from paymentsRow in registryContext.TenancyPayments
                            where tenancyIds.Contains(paymentsRow.IdProcess)
                            select paymentsRow).ToList();

            payments = (from paymentRow in payments
                        join tpRow in tenancyProcesses
                        on paymentRow.IdProcess equals tpRow.IdProcess
                        where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                            tpRow.TenancyPersons.Any()
                        select paymentRow).ToList();

            var prePaymentsAfter28082019Buildings = (from tbaRow in registryContext.TenancyBuildingsAssoc
                                                     join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                     on tbaRow.IdBuilding equals paymentRow.IdBuilding
                                                     where paymentRow.IdPremises == null && tenancyIds.Contains(tbaRow.IdProcess)
                                                     select new
                                                     {
                                                         tbaRow.IdProcess,
                                                         paymentRow.IdBuilding,
                                                         paymentRow.Hb,
                                                         paymentRow.K1,
                                                         paymentRow.K2,
                                                         paymentRow.K3,
                                                         paymentRow.KC,
                                                         RentArea = tbaRow.RentTotalArea == null ? paymentRow.RentArea : tbaRow.RentTotalArea
                                                     }).Distinct().ToList();

            var paymentsAfter28082019Buildings = (from paymentRow in prePaymentsAfter28082019Buildings
                                                  join tpRow in tenancyProcesses
                                                      on paymentRow.IdProcess equals tpRow.IdProcess
                                                  where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                    tpRow.TenancyPersons.Any()
                                                  select paymentRow).Distinct().ToList();

            var prePaymentsAfter28082019Premises = (from tpaRow in registryContext.TenancyPremisesAssoc
                                                    join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tpaRow.IdPremise equals paymentRow.IdPremises
                                                    where paymentRow.IdSubPremises == null && tenancyIds.Contains(tpaRow.IdProcess)
                                                    select new
                                                    {
                                                        tpaRow.IdProcess,
                                                        paymentRow.IdPremises,
                                                        paymentRow.Hb,
                                                        paymentRow.K1,
                                                        paymentRow.K2,
                                                        paymentRow.K3,
                                                        paymentRow.KC,
                                                        RentArea = tpaRow.RentTotalArea == null ? paymentRow.RentArea : tpaRow.RentTotalArea
                                                    }).Distinct().ToList();

            var paymentsAfter28082019Premises = (from paymentRow in prePaymentsAfter28082019Premises
                                                 join tpRow in tenancyProcesses
                                                     on paymentRow.IdProcess equals tpRow.IdProcess
                                                 where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                   tpRow.TenancyPersons.Any()
                                                 select paymentRow).Distinct().ToList();

            var prePaymentsAfter28082019SubPremises = (from tspaRow in registryContext.TenancySubPremisesAssoc
                                                       join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tspaRow.IdSubPremise equals paymentRow.IdSubPremises
                                                       where tenancyIds.Contains(tspaRow.IdProcess)
                                                       select new
                                                       {
                                                           tspaRow.IdProcess,
                                                           paymentRow.IdSubPremises,
                                                           paymentRow.Hb,
                                                           paymentRow.K1,
                                                           paymentRow.K2,
                                                           paymentRow.K3,
                                                           paymentRow.KC,
                                                           RentArea = tspaRow.RentTotalArea == null ? paymentRow.RentArea : tspaRow.RentTotalArea
                                                       }).Distinct().ToList();



            var paymentsAfter28082019SubPremises = (from paymentRow in prePaymentsAfter28082019SubPremises
                                                    join tpRow in tenancyProcesses
                                                        on paymentRow.IdProcess equals tpRow.IdProcess
                                                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                      tpRow.TenancyPersons.Any()
                                                    select paymentRow).Distinct().ToList();

            foreach (var obj in objects)
            {
                if (obj.RentObject.Address.AddressType == AddressTypes.Building)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdBuilding.ToString() == obj.RentObject.Address.Id && r.IdPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                       Math.Round(paymentsAfter28082019Buildings.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdBuilding.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.Premise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdPremises.ToString() == obj.RentObject.Address.Id && r.IdSubPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019Premises.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.SubPremise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdProcess == obj.IdProcess && r.IdSubPremises.ToString() == obj.RentObject.Address.Id).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019SubPremises.Where(
                            r => r.IdProcess == obj.IdProcess && r.IdSubPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
            }

            var result = new Dictionary<int, List<KumiAccountTenancyInfoVM>>();
            foreach (var accountId in accountIds)
            {
                if (!result.ContainsKey(accountId))
                    result.Add(accountId, new List<KumiAccountTenancyInfoVM>());
                var currentAccountTenancyAssoc = accountTenancyAssocs.Where(r => r.IdAccount == accountId);
                foreach(var currentAssoc in currentAccountTenancyAssoc)
                {
                    var tenancyProcess = tenancyProcesses.FirstOrDefault(r => r.IdProcess == currentAssoc.IdProcess);
                    var rentObjects = objects.Where(r => r.IdProcess == tenancyProcess.IdProcess).Select(r => r.RentObject).ToList();
                    var tenancyInfo = new KumiAccountTenancyInfoVM
                    {
                        RentObjects = rentObjects,
                        TenancyProcess = tenancyProcess,
                        Tenant = registryContext.TenancyPersons.FirstOrDefault(r => r.IdProcess == tenancyProcess.IdProcess && r.ExcludeDate == null && r.IdKinship == 1),
                        AccountAssoc = currentAssoc
                    };
                    result[accountId].Add(tenancyInfo);
                }
            }
            return result;
        }

        private Dictionary<int, List<ClaimInfo>> GetClaimsInfo(IEnumerable<KumiAccount> accounts)
        {
            var accountsIds = accounts.Select(r => r.IdAccount);
            var claims = registryContext.Claims.Where(c => c.IdAccountKumi != null && accountsIds.Contains(c.IdAccountKumi.Value));
            var claimIds = claims.Select(r => r.IdClaim);

            var claimLastStatesIds = from row in registryContext.ClaimStates
                                     where claimIds.Contains(row.IdClaim)
                                     group row.IdState by row.IdClaim into gs
                                     select new
                                     {
                                         IdClaim = gs.Key,
                                         IdState = gs.Max()
                                     };

            var claimsInfo = from claimLastStateRow in claimLastStatesIds
                             join claimStateRow in registryContext.ClaimStates.Where(cs => claimIds.Contains(cs.IdClaim))
                             on claimLastStateRow.IdState equals claimStateRow.IdState
                             join claimStateTypeRow in registryContext.ClaimStateTypes
                             on claimStateRow.IdStateType equals claimStateTypeRow.IdStateType
                             select new ClaimInfo
                             {
                                 IdClaim = claimStateRow.IdClaim,
                                 IdClaimCurrentState = claimStateTypeRow.IdStateType,
                                 ClaimCurrentState = claimStateTypeRow.StateType
                             };

            claimsInfo = from claimRow in claims
                         join accountId in accountsIds
                         on claimRow.IdAccountKumi equals accountId
                         join claimsInfoRow in claimsInfo
                         on claimRow.IdClaim equals claimsInfoRow.IdClaim into c
                         from cRow in c.DefaultIfEmpty()
                         select new ClaimInfo
                         {
                             IdClaim = claimRow.IdClaim,
                             StartDeptPeriod = claimRow.StartDeptPeriod,
                             EndDeptPeriod = claimRow.EndDeptPeriod,
                             IdAccount = accountId,
                             IdClaimCurrentState = cRow.IdClaimCurrentState,
                             ClaimCurrentState = cRow.ClaimCurrentState,
                             EndedForFilter = claimRow.EndedForFilter,
                             AmountTenancy = claimRow.AmountTenancy,
                             AmountPenalty = claimRow.AmountPenalties
                         };


            var result =
                    claimsInfo
                    .Select(c => new ClaimInfo
                    {
                        ClaimCurrentState = c.ClaimCurrentState,
                        IdClaimCurrentState = c.IdClaimCurrentState,
                        IdClaim = c.IdClaim,
                        StartDeptPeriod = c.StartDeptPeriod,
                        EndDeptPeriod = c.EndDeptPeriod,
                        EndedForFilter = c.EndedForFilter,
                        AmountTenancy = c.AmountTenancy,
                        AmountPenalty = c.AmountPenalty,
                        IdAccount = c.IdAccount
                    })
                    .GroupBy(r => r.IdAccount)
                    .Select(r => new { IdAccount = r.Key, Claims = r.OrderByDescending(v => v.IdClaim).Select(v => v) })
                    .ToDictionary(v => v.IdAccount, v => v.Claims.ToList());
            return result;
        }

        public KumiAccount GetKumiAccount(int idAccount)
        {
            var account = registryContext.KumiAccounts
                .Include(r => r.AccountsTenancyProcessesAssoc)
                .Include(r => r.Charges)
                .Include(r => r.Claims)
                .SingleOrDefault(a => a.IdAccount == idAccount);
            if (account == null) return null;
            foreach(var claim in account.Claims)
            {
                var currentClaimState = registryContext.ClaimStates.Include(r => r.IdStateTypeNavigation).Where(r => r.IdClaim == claim.IdClaim);
                claim.ClaimStates = currentClaimState.ToList();
            }
            account.Charges = account.Charges.Where(r => r.Hidden != 1).ToList();
            foreach (var charge in account.Charges)
            {
                var paymentCharges = registryContext.KumiPaymentCharges.Where(r => r.IdCharge == charge.IdCharge);
                charge.PaymentCharges = paymentCharges.ToList();
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

        public Executor CurrentExecutor
        {
            get
            {
                var userName = securityService.User?.UserName?.ToLowerInvariant();
                return registryContext.Executors.FirstOrDefault(e => e.ExecutorLogin != null &&
                                e.ExecutorLogin.ToLowerInvariant() == userName);
            }
        }
    }
}
