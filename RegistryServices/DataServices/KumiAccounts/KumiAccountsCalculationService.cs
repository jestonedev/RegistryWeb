using RegistryDb.Models;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.Classes;
using RegistryServices.Enums;
using RegistryServices.Models.KumiAccounts;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using RegistryDb.Models.Entities.Claims;
using Microsoft.EntityFrameworkCore;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryDb.Models.Entities.Tenancies;
using RegistryWeb.Enums;

namespace RegistryServices.DataServices.KumiAccounts
{
    public class KumiAccountsCalculationService
    {
        private readonly RegistryContext registryContext;
        private readonly KumiAccountsTenanciesService tenanciesService;

        public KumiAccountsCalculationService(RegistryContext registryContext, KumiAccountsTenanciesService tenanciesService)
        {
            this.registryContext = registryContext;
            this.tenanciesService = tenanciesService;
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
                }
                else
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

        public DateTime CorrectStartRewriteDate(DateTime startRewriteDate, DateTime startDate, List<KumiCharge> dbChargingInfo)
        {
            if (dbChargingInfo.Any(r => r.IsBksCharge == 1))
            {
                var minStartRewriteDate = dbChargingInfo.Where(r => r.IsBksCharge == 1).OrderBy(r => r.EndDate).Last().EndDate.AddDays(1);
                if (startRewriteDate < minStartRewriteDate) startRewriteDate = minStartRewriteDate;
            }
            return startRewriteDate;
        }

        public List<KumiCharge> CalcChargesInfo(KumiAccountInfoForPaymentCalculator account, List<KumiCharge> dbChargers, DateTime startDate, DateTime endDate, DateTime startRewriteDate, bool forceCalcCurrentPeriod = false)
        {
            if (startDate > endDate) throw new ApplicationException(
                string.Format("Дата начала расчета {0} превышает дату окончания завершенного периода {1}",
                startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy")));

            var charges = new List<KumiCharge>();
            var bksPenaltyMustBeInfo = new List<BksPenaltyMustBeInfo>();
            var chargesMustBeInfo = new List<KumiSumDateInfo>();
            while (startDate < endDate)
            {
                var subEndDate = startDate.AddMonths(1).AddDays(-1);
                var charge = CalcChargeInfo(account, startDate, subEndDate, charges, dbChargers, startRewriteDate, out bool outOfBound, forceCalcCurrentPeriod,
                    bksPenaltyMustBeInfo, chargesMustBeInfo);
                if (!outOfBound)
                    charges.Add(charge);
                startDate = startDate.AddMonths(1);
            }
            return charges;
        }

        public KumiCharge CalcChargeInfo(KumiAccountInfoForPaymentCalculator account, DateTime startDate, DateTime endDate,
            List<KumiCharge> prevCharges, List<KumiCharge> dbCharges, DateTime startRewriteDate, out bool outOfBound, bool forceCalcCurrentPerod,
            List<BksPenaltyMustBeInfo> bksPenaltyMustBeInfo, List<KumiSumDateInfo> chargesMustBeInfo)
        {
            // Входящее сальдо
            var inputBalanceTenancy = account.CurrentBalanceTenancy;
            var inputBalancePenalty = account.CurrentBalancePenalty;
            var inputBalanceDgi = account.CurrentBalanceDgi;
            var inputBalancePkk = account.CurrentBalancePkk;
            var inputBalancePadun = account.CurrentBalancePadun;

            KumiCharge prevCharge = null;
            if (prevCharges.Any())
                prevCharge = prevCharges.OrderByDescending(r => r.EndDate).First();

            if (prevCharge != null)
            {
                inputBalanceTenancy = prevCharge.OutputTenancy;
                inputBalancePenalty = prevCharge.OutputPenalty;
                inputBalanceDgi = prevCharge.OutputDgi;
                inputBalancePkk = prevCharge.OutputPkk;
                inputBalancePadun = prevCharge.OutputPadun;
            }
            else
            {
                var dbCharge = dbCharges.FirstOrDefault(r => r.IdAccount == account.IdAccount && r.StartDate == startDate && r.EndDate == endDate);
                if (dbCharge != null)
                {
                    inputBalanceTenancy = dbCharge.InputTenancy;
                    inputBalancePenalty = dbCharge.InputPenalty;
                    inputBalanceDgi = dbCharge.InputDgi;
                    inputBalancePkk = dbCharge.InputPkk;
                    inputBalancePadun = dbCharge.InputPadun;
                }
            }

            // Перенос перерасчетов из БД и начислений
            var recalcTenancy = 0m;
            var recalcPenalty = 0m;
            var recalcDgi = 0m;
            var recalcPkk = 0m;
            var recalcPadun = 0m;

            var chargeTenancy = 0m;
            var chargePenalty = 0m;
            var chargeDgi = 0m;
            var chargePkk = 0m;
            var chargePadun = 0m;

            KumiCharge currentSavedCharge = account.Charges.FirstOrDefault(r => r.StartDate == startDate && r.EndDate == endDate);
            var calcCharge = endDate < DateTime.Now.Date;
            if (currentSavedCharge != null)
            {
                calcCharge = calcCharge
                    || currentSavedCharge.ChargeTenancy != 0 || currentSavedCharge.ChargePenalty != 0 || currentSavedCharge.ChargeDgi != 0
                    || currentSavedCharge.ChargePkk != 0 || currentSavedCharge.ChargePadun != 0;
                // Если начисление не входит в период перезаписи, то перенести начисления как есть, иначе обнулить
                // Если ЛС не действующий, то перенести перерасчеты
                if (startRewriteDate > currentSavedCharge.StartDate || account.IdState != 1)
                {
                    recalcTenancy = currentSavedCharge.RecalcTenancy;
                    recalcDgi = currentSavedCharge.RecalcDgi;
                    recalcPkk = currentSavedCharge.RecalcPkk;
                    recalcPadun = currentSavedCharge.RecalcPadun;
                    if (account.IdState != 3)
                        recalcPenalty = currentSavedCharge.RecalcPenalty;
                }

                // Если лицевой счет не находится в статусе "Действующий", то переносим начисления, которые были сохранены в БД
                // Игнорируем будущие периоды. Также переносим плату за найм по начислениями БКС
                if (account.IdState != 1 || currentSavedCharge.IsBksCharge == 1)
                {
                    chargeTenancy = currentSavedCharge.ChargeTenancy;
                    chargePenalty = currentSavedCharge.ChargePenalty;
                    chargeDgi = currentSavedCharge.ChargeDgi;
                    chargePkk = currentSavedCharge.ChargePkk;
                    chargePadun = currentSavedCharge.ChargePadun;
                }
            }

            var correctionTenancy = 0m;
            var correctionPenalty = 0m;
            var correctionDgi = 0m;
            var correctionPkk = 0m;
            var correctionPadun = 0m;

            var paymentStartDate = startDate.AddMonths(-1).AddDays(24);
            var paymentEndDate = endDate.AddDays(1).AddMonths(-1).AddDays(23);

            // Если есть корректировки, учитываем их
            var corrections = account.Corrections.Where(r => r.Date >= paymentStartDate && r.Date <= paymentEndDate);
            correctionTenancy = corrections.Select(r => r.TenancyValue).Sum();
            correctionPenalty = corrections.Select(r => r.PenaltyValue).Sum();
            correctionDgi = corrections.Select(r => r.DgiValue).Sum();
            correctionPkk = corrections.Select(r => r.PkkValue).Sum();
            correctionPadun = corrections.Select(r => r.PadunValue).Sum();

            // Если лицевой счет в статусе "Действующий", то перерасчитываем начисления
            // Не начисляем за будущие периоды
            if (calcCharge || forceCalcCurrentPerod)
            {
                if (currentSavedCharge == null || currentSavedCharge.IsBksCharge != 1)
                {
                    var chargeStartDate = startDate;
                    var chargeEndDate = endDate;
                    if (account.StartChargeDate != null && account.StartChargeDate > chargeStartDate)
                        chargeStartDate = account.StartChargeDate.Value;
                    if (account.StopChargeDate != null && account.StopChargeDate < chargeEndDate)
                        chargeEndDate = account.StopChargeDate.Value;
                    if (chargeStartDate <= chargeEndDate)
                    {
                        var chargeMustBeInfo = 0m;
                        foreach (var tenancy in account.TenancyInfo)
                        {
                            chargeMustBeInfo += CalcChargeByTenancy(tenancy, chargeStartDate, chargeEndDate);
                        }
                        chargeMustBeInfo = Math.Round(chargeMustBeInfo, 2);
                        if (account.IdState == 1)
                            chargeTenancy = chargeMustBeInfo;
                        chargesMustBeInfo.Add(new KumiSumDateInfo
                        {
                            Date = endDate,
                            Value = chargeMustBeInfo
                        });
                    }
                }

                if (account.IdState == 1 || account.IdState == 3)
                {
                    var mustBeChargePenalty = Math.Round(CalcPenalty(account.Corrections, account.Charges, prevCharges, chargesMustBeInfo, account.Claims, account.Payments, endDate, startRewriteDate, bksPenaltyMustBeInfo), 2);
                    if (currentSavedCharge == null || currentSavedCharge.IsBksCharge != 1)
                    {
                        chargePenalty = mustBeChargePenalty;
                    }
                    else
                    {
                        bksPenaltyMustBeInfo.Add(new BksPenaltyMustBeInfo
                        {
                            StartDate = startDate,
                            EndDate = endDate,
                            ActualPenalty = chargePenalty,
                            MustBePenalty = mustBeChargePenalty
                        });
                    }
                }
            }

            var claimIds = account.Claims.Select(r => r.IdClaim).ToList();
            var chargeIds = account.Charges.Select(r => r.IdCharge).ToList();

            var paymentCharges = account.Payments.SelectMany(r => r.PaymentCharges).Where(r => r.Date >= paymentStartDate && r.Date <= paymentEndDate && chargeIds.Contains(r.IdCharge));
            var paymentClaims = account.Payments.SelectMany(r => r.PaymentClaims).Where(r => r.Date >= paymentStartDate && r.Date <= paymentEndDate && claimIds.Contains(r.IdClaim));

            var paymentPenalty = paymentCharges.Sum(r => r.PenaltyValue) + paymentClaims.Sum(r => r.PenaltyValue);
            var paymentTenancy = paymentCharges.Sum(r => r.TenancyValue) + paymentClaims.Sum(r => r.TenancyValue);
            var paymentDgi = paymentCharges.Sum(r => r.DgiValue) + paymentClaims.Sum(r => r.DgiValue);
            var paymentPkk = paymentCharges.Sum(r => r.PkkValue) + paymentClaims.Sum(r => r.PkkValue);
            var paymentPadun = paymentCharges.Sum(r => r.PadunValue) + paymentClaims.Sum(r => r.PadunValue);

            paymentTenancy += corrections.Select(r => r.PaymentTenancyValue).Sum();
            paymentPenalty += corrections.Select(r => r.PaymentPenaltyValue).Sum();
            paymentDgi += corrections.Select(r => r.PaymentDgiValue).Sum();
            paymentPkk += corrections.Select(r => r.PaymentPkkValue).Sum();
            paymentPadun += corrections.Select(r => r.PaymentPadunValue).Sum();

            outOfBound = false;

            return new KumiCharge
            {
                StartDate = startDate,
                EndDate = endDate,
                IdAccount = account.IdAccount,
                InputTenancy = inputBalanceTenancy,
                InputPenalty = inputBalancePenalty,
                InputDgi = inputBalanceDgi,
                InputPkk = inputBalancePkk,
                InputPadun = inputBalancePadun,
                ChargeTenancy = chargeTenancy,
                ChargePenalty = chargePenalty,
                ChargeDgi = chargeDgi,
                ChargePkk = chargePkk,
                ChargePadun = chargePadun,
                RecalcTenancy = recalcTenancy,
                RecalcPenalty = (currentSavedCharge != null && currentSavedCharge.IsBksCharge == 1) ? recalcPenalty : 0, //recalcPenalty,
                RecalcDgi = recalcDgi,
                RecalcPkk = recalcPkk,
                RecalcPadun = recalcPadun,
                CorrectionTenancy = correctionTenancy,
                CorrectionPenalty = correctionPenalty,
                CorrectionDgi = correctionDgi,
                CorrectionPkk = correctionPkk,
                CorrectionPadun = correctionPadun,
                PaymentTenancy = paymentTenancy,
                PaymentPenalty = paymentPenalty,
                PaymentDgi = paymentDgi,
                PaymentPkk = paymentPkk,
                PaymentPadun = paymentPadun,
                OutputTenancy = inputBalanceTenancy + chargeTenancy + recalcTenancy + correctionTenancy - paymentTenancy,
                OutputPenalty = inputBalancePenalty + chargePenalty + recalcPenalty + correctionPenalty - paymentPenalty,
                OutputDgi = inputBalanceDgi + chargeDgi + correctionDgi - paymentDgi,
                OutputPkk = inputBalancePkk + chargePkk + correctionPkk - paymentPkk,
                OutputPadun = inputBalancePadun + chargePadun + correctionPadun - paymentPadun,
                IsBksCharge = currentSavedCharge == null ? (byte)0 : currentSavedCharge.IsBksCharge
            };
        }

        public void CalcRecalcInfo(KumiAccountInfoForPaymentCalculator account, List<KumiCharge> chargingInfo,
            List<KumiCharge> dbChargingInfo, KumiCharge recalcInsertIntoCharge,
            DateTime startCalcDate, DateTime endCalcDate, DateTime startRewriteDate)
        {
            var accRecalcTenancy = 0m;
            var accRecalcPenalty = 0m;
            var accRecalcDgi = 0m;
            var accRecalcPkk = 0m;
            var accRecalcPadun = 0m;
            var accCorrectionTenancy = 0m;
            var accCorrectionPenalty = 0m;
            var accCorrectionDgi = 0m;
            var accCorrectionPkk = 0m;
            var accCorrectionPadun = 0m;
            var accPaymentTenancy = 0m;
            var accPaymentPenalty = 0m;
            var accPaymentDgi = 0m;
            var accPaymentPkk = 0m;
            var accPaymentPadun = 0m;
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
                    accRecalcDgi -= dbCharge.OutputDgi - dbCharge.InputDgi;
                    accRecalcPkk -= dbCharge.OutputPkk - dbCharge.InputPkk;
                    accRecalcPadun -= dbCharge.OutputPadun - dbCharge.InputPadun;
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

                var dbCharge = dbChargingInfo.FirstOrDefault(r => r.StartDate == charge.StartDate && r.EndDate == charge.EndDate);

                var dbNextCharge = dbChargingInfo.OrderBy(r => r.EndDate).FirstOrDefault(r => r.EndDate > charge.EndDate);
                var nextCharge = sortedChargingInfo.OrderBy(r => r.EndDate).FirstOrDefault(r => r.EndDate > charge.EndDate);

                // Если в БД нет соответствующего начисления, но это начисление не является последним в БД, то добавляем сумму в перерасчет
                if (dbCharge == null && dbNextCharge != null && startRewriteDate != charge.StartDate)
                {
                    accRecalcTenancy += charge.OutputTenancy - charge.InputTenancy;
                    accRecalcPenalty += charge.OutputPenalty - charge.InputPenalty;
                    accRecalcDgi += charge.OutputDgi - charge.InputDgi;
                    accRecalcPkk += charge.OutputPkk - charge.InputPkk;
                    accRecalcPadun += charge.OutputPadun - charge.InputPadun;
                }
                else
                // Признак Hidden присваивается заблокированным периодам, по которым производилась привязка платежей без начислений
                // Такие лицевые счета надо перерасчитывать полностью, чтобы отобразить платеж. В противном случае платеж пойдет в перерасчет
                if (dbCharge != null && dbCharge.Hidden == 1 && startRewriteDate != charge.StartDate)
                {
                    accPaymentTenancy += charge.PaymentTenancy;
                    accPaymentPenalty += charge.PaymentPenalty;
                    accPaymentDgi += charge.PaymentDgi;
                    accPaymentPkk += charge.PaymentPkk;
                    accPaymentPadun += charge.PaymentPadun;
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
                        accRecalcDgi += charge.ChargeDgi - dbCharge.ChargeDgi;
                        accRecalcPkk += charge.ChargePkk - dbCharge.ChargePkk;
                        accRecalcPadun += charge.ChargePadun - dbCharge.ChargePadun;

                        accCorrectionTenancy += charge.CorrectionTenancy - dbCharge.CorrectionTenancy;
                        accCorrectionPenalty += charge.CorrectionPenalty - dbCharge.CorrectionPenalty;
                        accCorrectionDgi += charge.CorrectionDgi - dbCharge.CorrectionDgi;
                        accCorrectionPkk += charge.CorrectionPkk - dbCharge.CorrectionPkk;
                        accCorrectionPadun += charge.CorrectionPadun - dbCharge.CorrectionPadun;

                        accPaymentTenancy += charge.PaymentTenancy - dbCharge.PaymentTenancy;
                        accPaymentPenalty += charge.PaymentPenalty - dbCharge.PaymentPenalty;
                        accPaymentDgi += charge.PaymentDgi - dbCharge.PaymentDgi;
                        accPaymentPkk += charge.PaymentPkk - dbCharge.PaymentPkk;
                        accPaymentPadun += charge.PaymentPadun - dbCharge.PaymentPadun;

                        // Для начислений не от БКС от общей суммы перерасчета необходимо отнять те перерасчеты, что уже учтены в предыдущих начислениях. 
                        // Для начислений БКС перерасчет переносится 1 к 1
                        if (dbCharge.IsBksCharge == 0)
                        {
                            accRecalcTenancy -= dbCharge.RecalcTenancy;
                            accRecalcPenalty -= dbCharge.RecalcPenalty;
                            accRecalcDgi -= dbCharge.RecalcDgi;
                            accRecalcPkk -= dbCharge.RecalcPkk;
                            accRecalcPadun -= dbCharge.RecalcPadun;
                        }
                    }
                }
                else
                if (dbCharge == null && charge.StartDate < startRewriteDate)
                {
                    accRecalcTenancy += charge.ChargeTenancy;
                    accRecalcPenalty += charge.ChargePenalty;
                    accRecalcDgi += charge.ChargeDgi;
                    accRecalcPkk += charge.ChargePkk;
                    accRecalcPadun += charge.ChargePadun;
                }
            }

            if (account.IdState == 1)
            {
                recalcInsertIntoCharge.RecalcTenancy = accRecalcTenancy;
                recalcInsertIntoCharge.RecalcDgi = accRecalcDgi;
                recalcInsertIntoCharge.RecalcPkk = accRecalcPkk;
                recalcInsertIntoCharge.RecalcPadun = accRecalcPadun;
            }

            if (account.IdState == 1 || account.IdState == 3)
                recalcInsertIntoCharge.RecalcPenalty = accRecalcPenalty;

            recalcInsertIntoCharge.CorrectionTenancy += accCorrectionTenancy;
            recalcInsertIntoCharge.CorrectionPenalty += accCorrectionPenalty;
            recalcInsertIntoCharge.CorrectionDgi += accCorrectionDgi;
            recalcInsertIntoCharge.CorrectionPkk += accCorrectionPkk;
            recalcInsertIntoCharge.CorrectionPadun += accCorrectionPadun;

            recalcInsertIntoCharge.PaymentTenancy += accPaymentTenancy;
            recalcInsertIntoCharge.PaymentPenalty += accPaymentPenalty;
            recalcInsertIntoCharge.PaymentDgi += accPaymentDgi;
            recalcInsertIntoCharge.PaymentPkk += accPaymentPkk;
            recalcInsertIntoCharge.PaymentPadun += accPaymentPadun;
        }

        public void RecalculateAccounts(List<int> accountIds, KumiAccountRecalcTypeEnum recalcType, DateTime? recalcStartDate, bool saveCurrentPeriodCharge)
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

            RecalculateAccounts(accounts, startRewriteDate, endCalcDate, saveCurrentPeriodCharge);
        }

        public KumiCharge CalcForecastChargeInfo(int idAccount, DateTime calcToDate)
        {
            DateTime? startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.Value.AddDays(-startRewriteDate.Value.Day + 1);
            if (DateTime.Now.Date.Day >= 25) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.Value.AddMonths(1);
            }

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

            startRewriteDate = CorrectStartRewriteDate(startRewriteDate.Value, startCalcDate.Value, dbChargingInfo);

            var chargingInfo = CalcChargesInfo(account, dbChargingInfo, startCalcDate.Value, calcToDate, startRewriteDate.Value, true);
            var recalcInsertIntoCharge = new KumiCharge();
            if (chargingInfo.Any()) recalcInsertIntoCharge = chargingInfo.Last();
            CalcRecalcInfo(account, chargingInfo, dbChargingInfo, recalcInsertIntoCharge, startCalcDate.Value, calcToDate, startRewriteDate.Value);
            return chargingInfo.FirstOrDefault(r => r.EndDate == calcToDate) ?? new KumiCharge();
        }

        public void RecalculateAccounts(IQueryable<KumiAccount> accounts, DateTime? startRewriteDate, DateTime endCalcDate, bool saveCurrentPeriodCharge)
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
                var forceCalcCurrentPeriod = saveCurrentPeriodCharge;
                var futureChargesInDb = dbChargingInfo.Where(r => r.EndDate >= DateTime.Now.Date &&
                    (r.ChargeTenancy != 0 || r.ChargePenalty != 0 || r.ChargePkk != 0 ||
                    r.ChargeDgi != 0 || r.ChargePadun != 0));
                if (futureChargesInDb.Any())
                {
                    forceCalcCurrentPeriod = true;
                    var futureChargesMaxDate = futureChargesInDb.Select(r => r.EndDate).Max();
                    if (futureChargesMaxDate > endCalcDate)
                        endCalcDate = futureChargesMaxDate;
                }

                startRewriteDate = CorrectStartRewriteDate(startRewriteDate.Value, startCalcDate.Value, dbChargingInfo);

                var chargingInfo = CalcChargesInfo(account, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate.Value, forceCalcCurrentPeriod);
                var recalcInsertIntoCharge = new KumiCharge();
                if (chargingInfo.Any()) recalcInsertIntoCharge = chargingInfo.Last();
                CalcRecalcInfo(account, chargingInfo, dbChargingInfo, recalcInsertIntoCharge, startCalcDate.Value, endCalcDate, startRewriteDate.Value);
                UpdateChargesIntoDb(account, chargingInfo, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate.Value,
                    saveCurrentPeriodCharge ? endCalcDate : (DateTime?)null);
            }
        }

        public List<KumiCharge> GetDbChargingInfo(KumiAccountInfoForPaymentCalculator account)
        {
            return registryContext.KumiCharges.AsNoTracking().Where(r => r.IdAccount == account.IdAccount).ToList();
        }

        public void UpdateChargesIntoDb(KumiAccountInfoForPaymentCalculator account, List<KumiCharge> chargingInfo,
            List<KumiCharge> dbChargingInfo,
            DateTime startCalcDate, DateTime endCalcDate, DateTime startRewriteDate, DateTime? calcDate, bool updateChargeDisplayInfo = true)
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
            var currentDgi = 0m;
            var currentPkk = 0m;
            var currentPadun = 0m;
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
                    // Если в БД нет начислений до даты перезаписи и предыдущее начисление не подпадает под перезапись, то считаем
                    // что начисление первое и берем нулевое сальдо
                    if (prevCharge == null || prevCharge.EndDate < startRewriteDate)
                    {
                        charge.InputTenancy = firstDbRewriteCharge?.InputTenancy ?? 0;
                        charge.InputPenalty = firstDbRewriteCharge?.InputPenalty ?? 0;
                        charge.InputDgi = firstDbRewriteCharge?.InputDgi ?? 0;
                        charge.InputPkk = firstDbRewriteCharge?.InputPkk ?? 0;
                        charge.InputPadun = firstDbRewriteCharge?.InputPadun ?? 0;
                    }
                    else
                    // Если предыдущее начисление подпадает под перезапись, то берем сальдо из него
                    {
                        charge.InputTenancy = prevCharge.OutputTenancy;
                        charge.InputPenalty = prevCharge.OutputPenalty;
                        charge.InputDgi = prevCharge.OutputDgi;
                        charge.InputPkk = prevCharge.OutputPkk;
                        charge.InputPadun = prevCharge.OutputPadun;
                    }
                }
                else
                // Если в БД есть начисления, которые не попали под перезапись, то берем сальдо из него
                if (prevCharge != null)
                {
                    if (lastDbLockedCharge.StartDate >= prevCharge.StartDate || prevCharge.EndDate < startRewriteDate)
                    {
                        charge.InputTenancy = lastDbLockedCharge.OutputTenancy;
                        charge.InputPenalty = lastDbLockedCharge.OutputPenalty;
                        charge.InputDgi = lastDbLockedCharge.OutputDgi;
                        charge.InputPkk = lastDbLockedCharge.OutputPkk;
                        charge.InputPadun = lastDbLockedCharge.OutputPadun;
                        lastDbLockedCharge = null;
                    }
                    else
                    {
                        charge.InputTenancy = prevCharge.OutputTenancy;
                        charge.InputPenalty = prevCharge.OutputPenalty;
                        charge.InputDgi = prevCharge.OutputDgi;
                        charge.InputPkk = prevCharge.OutputPkk;
                        charge.InputPadun = prevCharge.OutputPadun;
                    }
                }

                charge.OutputTenancy = charge.InputTenancy + charge.ChargeTenancy - charge.PaymentTenancy + charge.RecalcTenancy + charge.CorrectionTenancy;
                charge.OutputPenalty = charge.InputPenalty + charge.ChargePenalty - charge.PaymentPenalty + charge.RecalcPenalty + charge.CorrectionPenalty;
                charge.OutputDgi = charge.InputDgi + charge.ChargeDgi - charge.PaymentDgi + charge.CorrectionDgi;
                charge.OutputPkk = charge.InputPkk + charge.ChargePkk - charge.PaymentPkk + charge.CorrectionPkk;
                charge.OutputPadun = charge.InputPadun + charge.ChargePadun - charge.PaymentPadun + charge.CorrectionPadun;

                /*var notAddCharge = nextCharge == null && 
                    charge.PaymentTenancy == 0 && charge.PaymentPenalty == 0 && charge.PaymentDgi == 0 && charge.PaymentPkk == 0 && charge.PaymentPadun == 0 &&
                    charge.ChargeTenancy == 0 && charge.ChargePenalty == 0 && charge.ChargeDgi == 0 && charge.ChargePkk == 0 && charge.ChargePadun == 0 &&
                    charge.RecalcTenancy == 0 && charge.RecalcPenalty == 0 && charge.RecalcDgi == 0 && charge.RecalcPkk == 0 && charge.RecalcPadun == 0 &&
                    charge.CorrectionTenancy == 0 && charge.CorrectionPenalty == 0 && charge.CorrectionDgi == 0 && charge.CorrectionPkk == 0 && charge.CorrectionPadun == 0;*/
                var notAddCharge = false;

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
                    }
                    else
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
                    currentDgi = charge.OutputDgi;
                    currentPkk = charge.OutputPkk;
                    currentPadun = charge.OutputPadun;
                    lastChargingDate = chargingInfo.Where(r => r.ChargePenalty != 0 || r.ChargeTenancy != 0 || r.ChargeDgi != 0 || r.ChargePkk != 0 || r.ChargePadun != 0)
                        .OrderByDescending(r => r.EndDate).FirstOrDefault()?.EndDate;
                }
            }
            var accountDb = registryContext.KumiAccounts.FirstOrDefault(r => r.IdAccount == account.IdAccount);
            accountDb.CurrentBalanceTenancy = currentTenancy;
            accountDb.CurrentBalancePenalty = currentPenalty;
            accountDb.CurrentBalanceDgi = currentDgi;
            accountDb.CurrentBalancePkk = currentPkk;
            accountDb.CurrentBalancePadun = currentPadun;
            accountDb.LastChargeDate = lastChargingDate;
            if (calcDate != null)
                accountDb.LastCalcDate = calcDate;
            else
                accountDb.LastCalcDate = (accountDb.LastCalcDate != null && accountDb.LastCalcDate > DateTime.Now.Date) ? accountDb.LastCalcDate : DateTime.Now.Date;
            accountDb.RecalcMarker = 0;
            accountDb.RecalcReason = null;
            registryContext.SaveChanges();
            if (updateChargeDisplayInfo)
                UpdateChargeDisplayInfo(accountDb.IdAccount, startRewriteDate);
        }

        private void UpdateChargeDisplayInfo(int idAccount, DateTime startRewriteDate)
        {
            var charges = registryContext.KumiCharges.Include(r => r.PaymentCharges).Where(r => r.IdAccount == idAccount).ToList();
            // Если строк начисления нет в БД, то нечего обновлять
            if (charges.Count() == 0) return;
            var lastCharge = charges.OrderByDescending(r => r.EndDate).First();
            var claims = registryContext.Claims.Include(r => r.PaymentClaims).Where(r => r.IdAccountKumi == idAccount).ToList();
            foreach (var charge in charges)
            {
                foreach (var paymentCharge in charge.PaymentCharges)
                {
                    // Если начисление до даты перезаписи и указан IdDisplayCharge, то оставляем как есть
                    // Если начисление до даты перезаписи и IdDisplayCharge == null, то ставим идентификатор последнего начисления
                    if (paymentCharge.IdDisplayCharge == null && charge.StartDate < startRewriteDate)
                    {
                        paymentCharge.IdDisplayCharge = lastCharge.IdCharge;
                    }
                    else
                    // Если начисление после даты перезаписи, то ставим идентификатор текущего начисления
                    if (charge.StartDate >= startRewriteDate)
                    {
                        paymentCharge.IdDisplayCharge = paymentCharge.IdCharge;
                    }
                }
            }
            var paymentIds = claims.SelectMany(r => r.PaymentClaims).Select(r => r.IdPayment).Distinct();
            var payments = registryContext.KumiPayments.Where(r => paymentIds.Contains(r.IdPayment)).ToList();
            foreach (var claim in claims)
            {
                foreach (var paymentClaim in claim.PaymentClaims)
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

        internal List<KumiSumDateInfo> GetChargesInfoForCalcPenalty(List<KumiChargeCorrection> corrections,
            List<KumiCharge> dbCharges, List<KumiCharge> charges, DateTime? endDate, DateTime startRewriteDate, List<KumiSumDateInfo> chargesMustBeInfo = null)
        {
            var chargeCorrections = corrections.Where(r => r.TenancyValue > 0)
                .Select(r => new KumiSumDateInfo
                {
                    Date = r.Date,
                    Value = r.TenancyValue
                });

            DateTime? splitDate = null;
            if (dbCharges.Any(r => r.IsBksCharge == 1))
                splitDate = dbCharges.Where(r => r.IsBksCharge == 1).Select(r => r.StartDate).Max();

            var resultCharges =
                dbCharges.Where(r => splitDate != null && r.StartDate <= splitDate && r.EndDate <= endDate);

            if (chargesMustBeInfo == null)
                resultCharges = resultCharges.Union(charges.Where(r => ((splitDate != null && r.StartDate > splitDate) || splitDate == null) && r.EndDate <= endDate));

            var firstChargeInputInfo = resultCharges.OrderBy(r => r.EndDate).FirstOrDefault();

            return resultCharges
                .Select(r => new KumiSumDateInfo
                {
                    Date = r.EndDate,
                    Value = firstChargeInputInfo != null && firstChargeInputInfo.IsBksCharge == 1 && firstChargeInputInfo.EndDate == r.EndDate ?
                        r.ChargeTenancy + (r.IsBksCharge == 1 ? r.RecalcTenancy : 0) + firstChargeInputInfo.InputTenancy :
                        r.ChargeTenancy + (r.IsBksCharge == 1 ? r.RecalcTenancy : 0)
                }).Union(chargeCorrections).Union(chargesMustBeInfo == null ?
                    new List<KumiSumDateInfo>() :
                    chargesMustBeInfo.Select(cmb => new KumiSumDateInfo { Date = cmb.Date, Value = cmb.Value })).ToList();
        }

        internal List<KumiSumDateInfo> GetPaymentsInfoForCalcPenalty(List<KumiChargeCorrection> corrections,
            List<KumiCharge> dbCharges, List<Claim> claims,
            List<KumiPayment> payments, DateTime? endDate)
        {
            var chargeIds = dbCharges.Select(r => r.IdCharge).ToList();
            var claimIds = claims.Select(r => r.IdClaim).ToList();

            var calcPayments = payments.Where(r =>
                r.DateExecute != null ? r.DateExecute <= endDate :
                r.DateIn != null ? r.DateIn <= endDate :
                r.DateDocument != null ? r.DateDocument <= endDate : false)
                .Where(r => r.PaymentCharges.Any(pc => chargeIds.Contains(pc.IdCharge)) || r.PaymentClaims.Any(pc => claimIds.Contains(pc.IdClaim)));

            var preparedPayments = calcPayments.Select(r => new KumiSumDateInfo
            {
                Date = (r.DateExecute ?? r.DateIn ?? r.DateDocument).Value,
                Value = r.PaymentCharges.Where(pc => chargeIds.Contains(pc.IdCharge)).Sum(pc => pc.TenancyValue) +
                     r.PaymentClaims.Where(pc => claimIds.Contains(pc.IdClaim)).Sum(pc => pc.TenancyValue)
            });

            var paymentCorrections = corrections.Where(r => r.PaymentTenancyValue != 0)
                .Select(r => new KumiSumDateInfo
                {
                    Date = r.Date,
                    Value = r.PaymentTenancyValue
                });

            var negativeCharges = corrections.Where(r => r.TenancyValue < 0)
                .Select(r => new KumiSumDateInfo
                {
                    Date = r.Date,
                    Value = -r.TenancyValue
                });

            return preparedPayments.Union(paymentCorrections).Union(negativeCharges).ToList();
        }

        private decimal CalcPenalty(List<KumiChargeCorrection> corrections, List<KumiCharge> dbCharges, List<KumiCharge> charges,
            List<KumiSumDateInfo> chargesMustBeInfo, List<Claim> claims,
            List<KumiPayment> payments, DateTime? endDate, DateTime startRewriteDate,
            List<BksPenaltyMustBeInfo> bksPenaltyMustBeInfo)
        {
            var resultPayments = GetPaymentsInfoForCalcPenalty(corrections, dbCharges, claims, payments, endDate);
            var resultChargesInfo = GetChargesInfoForCalcPenalty(corrections, dbCharges, charges, endDate, startRewriteDate, chargesMustBeInfo);

            var sliceDate = new DateTime(2023, 7, 1).AddDays(-11);
            var aggCharges = resultChargesInfo.Where(r => r.Date <= sliceDate);
            resultPayments = resultPayments.Where(r => r.Date > sliceDate).ToList();
            resultChargesInfo = resultChargesInfo.Where(r => r.Date > sliceDate).ToList();
            if (aggCharges.Any())
            {
                var dbCharge = dbCharges.Where(r => r.StartDate <= sliceDate).OrderByDescending(r => r.StartDate).FirstOrDefault();
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

            var penalty = 0m;

            // Расчет пени
            while (resultChargesInfo.Where(r => r.Value != 0).Any() && resultPayments.Where(r => r.Value != 0).Any())
            {
                var firstCharge = resultChargesInfo.Where(r => r.Value != 0).OrderBy(r => r.Date).First();
                var dbCharge = dbCharges.Where(r => r.EndDate == firstCharge.Date).FirstOrDefault();
                while (firstCharge.Value != 0 && resultPayments.Where(r => r.Value != 0).Any())
                {
                    var firstPayment = resultPayments.Where(r => r.Value != 0).OrderBy(r => r.Date).First();
                    var calcSum = 0m;
                    if (firstCharge.Value >= firstPayment.Value)
                    {
                        firstCharge.Value -= firstPayment.Value;
                        calcSum = firstPayment.Value;
                        firstPayment.Value = 0;
                    }
                    else
                    {
                        firstPayment.Value -= firstCharge.Value;
                        calcSum = firstCharge.Value;
                        firstCharge.Value = 0;
                    }
                    penalty += CalcPenalty(firstCharge.Date, firstPayment.Date, calcSum, out List<KumiActPeniCalcEventVM> peniCalcEvents);
                }
            }

            foreach (var charge in resultChargesInfo.Where(r => r.Value > 0))
            {
                var dbCharge = dbCharges.Where(r => r.EndDate == charge.Date).FirstOrDefault();
                penalty += CalcPenalty(charge.Date, endDate.Value, charge.Value, out List<KumiActPeniCalcEventVM> peniCalcEvents);
            }

            // Учет итогового пени за все периоды (включая нерасчетного пени от БКС) для последующего вычитания предыдущих периодов
            var prevPenalty = charges.Where(r => r.EndDate < endDate && r.IsBksCharge == 0).Sum(r => r.ChargePenalty + r.RecalcPenalty);
            prevPenalty += bksPenaltyMustBeInfo.Where(r => r.EndDate < endDate).Select(r => r.MustBePenalty).Sum();
            //prevPenalty += corrections.Where(r => r.Date < endDate).Select(r => r.PenaltyValue).Sum();
            return penalty - prevPenalty;
        }

        internal decimal CalcPenalty(DateTime chargeDate, DateTime paymentDate, decimal sum, out List<KumiActPeniCalcEventVM> peniCalcEvents)
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
                    KeyRateCoef = 1 / 300m,
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

            foreach (var penaltyInfo in penaltyCalcInfo)
            {
                var currentPenalty = ((penaltyInfo.EndDate - penaltyInfo.StartDate).Days + 1) * (penaltyInfo.KeyRate / 100m) * penaltyInfo.KeyRateCoef * penaltyInfo.Sum;
                peniCalcEvents.Add(new KumiActPeniCalcEventVM
                {
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

        public List<KumiAccountPrepareForPaymentCalculator> GetAccountsPrepareForPaymentCalculator(IQueryable<KumiAccount> accounts)
        {
            var result = new List<KumiAccountPrepareForPaymentCalculator>();
            var tenancyInfo = tenanciesService.GetTenancyInfo(accounts, true);
            foreach (var account in accounts.Include(r => r.Charges).Include(r => r.Claims).Include(r => r.Corrections).AsNoTracking().ToList())
            {
                var accountInfo = new KumiAccountPrepareForPaymentCalculator();
                accountInfo.Account = account;
                if (tenancyInfo.ContainsKey(account.IdAccount))
                {
                    accountInfo.TenancyInfo = tenancyInfo[account.IdAccount];
                    accountInfo.TenancyPaymentHistories = new Dictionary<int, List<TenancyPaymentHistory>>();
                    foreach (var tenancy in accountInfo.TenancyInfo)
                    {
                        accountInfo.TenancyPaymentHistories.Add(tenancy.TenancyProcess.IdProcess,
                            tenancy.RentObjects.SelectMany(r => r.TenancyPaymentHistory).ToList());
                    }
                }
                var chargesIds = account.Charges.Select(r => r.IdCharge).ToList();
                var claimsIds = account.Claims.Select(r => r.IdClaim).ToList();

                var paymentIdsByCharges = registryContext.KumiPaymentCharges.Where(r => chargesIds.Contains(r.IdCharge)).Select(r => r.IdPayment).ToList();
                var paymentIdsByClaims = registryContext.KumiPaymentClaims.Where(r => claimsIds.Contains(r.IdClaim)).Select(r => r.IdPayment).ToList();
                accountInfo.Payments = registryContext.KumiPayments.Include(p => p.PaymentCharges)
                    .Where(p => paymentIdsByCharges.Contains(p.IdPayment)).ToList().Union(
                    registryContext.KumiPayments.Include(p => p.PaymentClaims)
                    .Where(p => paymentIdsByClaims.Contains(p.IdPayment)).ToList()
                    ).ToList();

                // Платежи по начислениям БКС (замороженные строки без фактических платежей)
                var bksPayments = account.Charges.Where(r => r.IsBksCharge == 1 &&
                        (r.PaymentTenancy != 0 || r.PaymentPenalty != 0 || r.PaymentDgi != 0 || r.PaymentPkk != 0 || r.PaymentPadun != 0))
                    .Select(r => new KumiPayment
                    {
                        DateIn = r.StartDate,
                        DateExecute = r.StartDate,
                        DateDocument = r.StartDate,
                        Sum = r.PaymentTenancy + r.PaymentPenalty + r.PaymentDgi + r.PaymentPkk + r.PaymentPadun,
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
                            new KumiPaymentCharge {
                                Date = r.StartDate,
                                DgiValue = r.PaymentDgi,
                                IdCharge = r.IdCharge,
                                IdDisplayCharge = r.IdCharge,
                            },
                            new KumiPaymentCharge {
                                Date = r.StartDate,
                                PkkValue = r.PaymentPkk,
                                IdCharge = r.IdCharge,
                                IdDisplayCharge = r.IdCharge,
                            },
                            new KumiPaymentCharge {
                                Date = r.StartDate,
                                PadunValue = r.PaymentPadun,
                                IdCharge = r.IdCharge,
                                IdDisplayCharge = r.IdCharge,
                            }
                        }
                    });

                accountInfo.Payments.AddRange(bksPayments);

                // Исковые работы
                accountInfo.Claims = account.Claims.ToList();
                var claimsStates = registryContext.ClaimStates.Where(r => claimsIds.Contains(r.IdClaim)).ToList();
                foreach (var claim in accountInfo.Claims)
                {
                    claim.ClaimStates = claimsStates.Where(r => r.IdClaim == claim.IdClaim).ToList();
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
            foreach (var account in accounts)
            {
                var accountInfo = new KumiAccountInfoForPaymentCalculator
                {
                    IdAccount = account.Account.IdAccount,
                    IdState = account.Account.IdState,
                    Account = account.Account.Account,
                    LastChargeDate = account.Account.LastChargeDate,
                    StartChargeDate = account.Account.StartChargeDate,
                    StopChargeDate = account.Account.StopChargeDate,
                    CurrentBalanceTenancy = account.Account.CurrentBalanceTenancy ?? 0,
                    CurrentBalancePenalty = account.Account.CurrentBalancePenalty ?? 0,
                    CurrentBalanceDgi = account.Account.CurrentBalanceDgi ?? 0,
                    CurrentBalancePkk = account.Account.CurrentBalancePkk ?? 0,
                    CurrentBalancePadun = account.Account.CurrentBalancePadun ?? 0
                };
                accountInfo.Payments = account.Payments;
                accountInfo.Claims = account.Claims;
                accountInfo.Charges = account.Account.Charges.ToList();
                accountInfo.Corrections = account.Account.Corrections.ToList();

                accountInfo.TenancyInfo = new List<KumiTenancyInfoForPaymentCalculator>();
                foreach (var tenancy in account.TenancyInfo)
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
                        payment.FromDate = DateTime.Now.Date.AddDays(-DateTime.Now.Date.Day + 1).AddMonths(-1);
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
                if (nextPeriod == null && process.AnnualDate != null && (currentPeriod.ToDate == null || currentPeriod.ToDate > process.AnnualDate))
                {
                    currentPeriod.ToDate = process.AnnualDate;
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
            foreach (var rentPaymentInfo in tenancy.RentPayments.GroupBy(r => new { r.IdObject, r.AddressType }))
            {
                var actualPaymentPeriods = rentPaymentInfo.Where(r => r.FromDate <= endDate && r.FromDate >= startDate).ToList();
                var prevStartPaymentPeriod = rentPaymentInfo.Where(r => r.FromDate < startDate).OrderByDescending(r => r.FromDate).FirstOrDefault();
                if (prevStartPaymentPeriod != null)
                {
                    actualPaymentPeriods.Add(prevStartPaymentPeriod);
                }
                var paymentPeriods = new List<RentSubPaymentForPaymentCalculator>();
                foreach (var actualPaymentPeriod in actualPaymentPeriods)
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
                        }
                        else
                        if (paymentPeriod.FromDate <= rentToDate && paymentPeriod.ToDate >= rentToDate) // Пересечение правой границы
                        {
                            from = paymentPeriod.FromDate;
                            to = rentToDate.Value;
                        }
                        else
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

        private List<KumiPenaltyCalcInfo> UpdatePenaltyCalcInfoKeyRate(List<KumiPenaltyCalcInfo> penaltyCalcInfo)
        {
            var result = new List<KumiPenaltyCalcInfo>();

            foreach (var penaltyInfo in penaltyCalcInfo)
            {
                var prevKeyRate = registryContext.KumiKeyRates.Where(r => r.StartDate <= penaltyInfo.StartDate)
                    .OrderByDescending(r => r.StartDate).FirstOrDefault();
                var inPeriodKeyRates = registryContext.KumiKeyRates.Where(r => r.StartDate > penaltyInfo.StartDate && r.StartDate <= penaltyInfo.EndDate).ToList();
                if (prevKeyRate != null)
                {
                    inPeriodKeyRates.Add(prevKeyRate);
                }
                foreach (var keyRate in inPeriodKeyRates.OrderBy(r => r.StartDate))
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

    }
}
