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
                if (firstCharge.InputTenancy > 0 || firstCharge.InputPenalty > 0)
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

        public List<KumiCharge> CalcChargesInfo(KumiAccountInfoForPaymentCalculator account, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate) throw new ApplicationException(
                string.Format("Дата начала расчета {0} превышает дату окончания завершенного периода {1}",
                startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy")));

            var charges = new List<KumiCharge>();

            while(startDate < endDate)
            {
                var subEndDate = startDate.AddMonths(1).AddDays(-1);
                var charge = CalcChargeInfo(account, startDate, subEndDate, charges, out bool outOfBound);
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
            foreach (var dbCharge in dbChargingInfo)
            {
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
                    accRecalcPenalty += charge.OutputPenalty - charge.InputPenalty;
                    accRecalcTenancy += charge.OutputTenancy - charge.InputTenancy;
                }
                else
                // Признак Hidden присваивается заблокированным периодам, по которым производилась привязка платежей без начислений
                // Такие лицевые счета надо перерасчитывать полностью, чтобы отобразить платеж. В противном случае платеж пойдет в перерасчет
                if (dbCharge != null && dbCharge.Hidden == 1 && startRewriteDate != charge.StartDate)
                {
                    accRecalcTenancy -= charge.PaymentTenancy;
                    accRecalcPenalty -= charge.PaymentPenalty;
                }
                else
                if (dbCharge != null && nextCharge != null)
                {
                    if (startRewriteDate != charge.StartDate)
                    {
                        accRecalcTenancy += charge.ChargeTenancy - dbCharge.ChargeTenancy -
                            (charge.PaymentTenancy - dbCharge.PaymentTenancy);
                        accRecalcPenalty += charge.ChargePenalty - dbCharge.ChargePenalty -
                            (charge.PaymentPenalty - dbCharge.PaymentPenalty);
                    }
                    accRecalcTenancy -= dbCharge.RecalcTenancy;
                    accRecalcPenalty -= dbCharge.RecalcPenalty;
                }
            }

            recalcInsertIntoCharge.RecalcTenancy = accRecalcTenancy;
            recalcInsertIntoCharge.RecalcPenalty = accRecalcPenalty;
        }

        public DateTime CorrectStartRewriteDate(DateTime startRewriteDate, DateTime startDate, List<KumiCharge> dbChargingInfo)
        {
            if (!dbChargingInfo.Where(r => r.Hidden == 0).Any())
                return startDate;
            else
            {
                var lastChargeEndDate = dbChargingInfo.Where(r => r.Hidden == 0).OrderBy(r => r.EndDate).Last().EndDate;
                var maxStartRewriteDate = lastChargeEndDate.AddDays(1);
                if (maxStartRewriteDate < startRewriteDate) return maxStartRewriteDate;
            }
            return startRewriteDate;
        }

        public void RecalculateAccounts(List<int> accountIds, KumiAccountRecalcTypeEnum recalcType, DateTime? recalcStartDate)
        {
            var accounts = registryContext.KumiAccounts.Where(r => accountIds.Contains(r.IdAccount) && r.IdState != 2);

            if (accounts.Count() == 0) return;

            DateTime? startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.Value.AddDays(-startRewriteDate.Value.Day + 1);
            if (DateTime.Now.Date.Day <= 3) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.Value.AddMonths(-1);
            }
            if (recalcType == KumiAccountRecalcTypeEnum.RewriteCharge)
            {
                startRewriteDate = recalcStartDate;
            }

            var endCalcDate = DateTime.Now.Date;
            endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);

            RecalculateAccounts(accounts, startRewriteDate, endCalcDate);
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
                var chargingInfo = CalcChargesInfo(account, startCalcDate.Value, endCalcDate);
                var recalcInsertIntoCharge = new KumiCharge();
                if (chargingInfo.Any()) recalcInsertIntoCharge = chargingInfo.Last();

                var dbChargingInfo = GetDbChargingInfo(account);
                startRewriteDate = CorrectStartRewriteDate(startRewriteDate.Value, startCalcDate.Value, dbChargingInfo);
                CalcRecalcInfo(account, chargingInfo, dbChargingInfo, recalcInsertIntoCharge, startCalcDate.Value, endCalcDate, startRewriteDate.Value);
                UpdateChargesIntoDb(account, chargingInfo, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate.Value);
            }
        }

        public List<KumiCharge> GetDbChargingInfo(KumiAccountInfoForPaymentCalculator account)
        {
            return registryContext.KumiCharges.AsNoTracking().Where(r => r.IdAccount == account.IdAccount).ToList();
        }

        public void UpdateChargesIntoDb(KumiAccountInfoForPaymentCalculator account, List<KumiCharge> chargingInfo,
            List<KumiCharge> dbChargingInfo,
            DateTime startCalcDate, DateTime endCalcDate, DateTime startRewriteDate)
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
            accountDb.RecalcMarker = 0;
            accountDb.RecalcReason = null;
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
                var tenancyProcesses = (from tpRow in registryContext.TenancyProcesses
                                    .Where(tp => (tp.RegistrationNum == null || !tp.RegistrationNum.Contains("н")) && tp.IdAccount == idAccount)
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
            List<KumiCharge> prevCharges, out bool outOfBound)
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
                if (endDate < DateTime.Now.Date && account.IdState != 1)
                {
                    chargeTenancy = currentSavedCharge.ChargeTenancy;
                    chargePenalty = currentSavedCharge.ChargePenalty;
                }
            }
            // Если лицевой счет в статусе "Действующий", то перерасчитываем начисления
            // Не начисляем за будущие периоды
            if (endDate < DateTime.Now.Date)
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
                    chargePenalty = Math.Round(CalcPenalty(account.Charges, prevCharges, account.Claims, account.Payments, endDate), 2);
                }
            }

            var payments = account.Payments.Where(r =>
                r.DateExecute != null ? r.DateExecute >= startDate && r.DateExecute <= endDate :
                r.DateIn != null ? r.DateIn >= startDate && r.DateIn <= endDate :
                r.DateDocument != null ? r.DateDocument >= startDate && r.DateDocument <= endDate : false);

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
                PaymentTenancy = paymentTenancy,
                PaymentPenalty = paymentPenalty,
                OutputTenancy = inputBalanceTenancy + chargeTenancy + recalcTenancy - paymentTenancy,
                OutputPenalty = inputBalancePenalty + chargePenalty + recalcPenalty - paymentPenalty,
            };
        }

        private decimal CalcPenalty(List<KumiCharge> dbCharges, List<KumiCharge> charges, List<Claim> claims, List<KumiPayment> payments, DateTime? endDate)
        {
            var chargeIds = dbCharges.Select(r => r.IdCharge).ToList();

            var calcPayments = payments.Where(r =>
                r.DateExecute != null ? r.DateExecute <= endDate :
                r.DateIn != null ? r.DateIn <= endDate :
                r.DateDocument != null ? r.DateDocument <= endDate : false).Where(r => r.PaymentCharges.Any(pc => chargeIds.Contains(pc.IdCharge)));

            var preparedClaims = claims.Where(r =>
                    r.ClaimStates.Any(s => s.IdStateType == 4 && s.CourtOrderDate != null) &&
                    !r.ClaimStates.Any(s => s.IdStateType == 6 && s.CourtOrderCancelDate != null))
                .Select(r => new KumiSumDateInfo
                {
                    Date = r.ClaimStates.FirstOrDefault(s => s.IdStateType == 4).CourtOrderDate.Value,
                    Value = (r.AmountTenancy + r.AmountPkk + r.AmountPadun + r.AmountDgi) ?? 0
                });

            var preparedPayments = calcPayments.Select(r => new KumiSumDateInfo
            {
                Date = (r.DateExecute ?? r.DateIn ?? r.DateDocument).Value,
                Value = r.PaymentCharges.Where(pc => chargeIds.Contains(pc.IdCharge)).Sum(pc=> pc.TenancyValue)
            });

            var resultPayments = preparedClaims.Union(preparedPayments).ToList();

            var resultCharges = charges.Where(r => r.EndDate <= endDate).Select(r => new KumiSumDateInfo
            {
                Date = r.EndDate,
                Value = r.ChargeTenancy + r.RecalcTenancy
            }).ToList();

            var penalty = 0m;

            while(resultCharges.Where(r => r.Value > 0).Any() && resultPayments.Where(r => r.Value > 0).Any())
            {
                var firstCharge = resultCharges.Where(r => r.Value > 0).OrderBy(r => r.Date).First();
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
                    penalty += CalcPenalty(firstCharge.Date, firstPayment.Date, calcSum);
                }
            }

            foreach(var charge in resultCharges.Where(r => r.Value > 0))
            {
                penalty += CalcPenalty(charge.Date, endDate.Value, charge.Value);
            }

            var prevPenalty = charges.Where(r => r.EndDate < endDate).Sum(r => r.ChargePenalty + r.RecalcPenalty);

            return penalty- prevPenalty;
        }

        private decimal CalcPenalty(DateTime chargeDate, DateTime paymentDate, decimal sum)
        {
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
                penalty += ((penaltyInfo.EndDate - penaltyInfo.StartDate).Days + 1) * (penaltyInfo.KeyRate/100m) * penaltyInfo.KeyRateCoef * penaltyInfo.Sum;
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
            foreach(var account in accounts.Include(r => r.Charges).Include(r => r.Claims).AsNoTracking().ToList())
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
                        accountInfo.TenancyPaymentHistories.Add(tenancy.TenancyProcess.IdProcess, paymentsHistory);
                    }
                }
                var chargesIds = account.Charges.Select(r => r.IdCharge).ToList();
                var claimsIds = account.Claims.Select(r => r.IdClaim).ToList();
                accountInfo.Payments = registryContext.KumiPayments.Include(p => p.PaymentCharges)
                    .Where(p => p.PaymentCharges.Any(r => chargesIds.Contains(r.IdCharge))).ToList()
                    .Union(
                        registryContext.KumiPayments.Include(p => p.PaymentClaims)
                        .Where(p => p.PaymentClaims.Any(r => claimsIds.Contains(r.IdClaim))).ToList())
                    .ToList();
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
                    var payment = new RentPaymentForPaymentCalculator();
                    if (historyInfo.IdSubPremises != null)
                    {
                        payment.AddressType = AddressTypes.SubPremise;
                        payment.IdObject = historyInfo.IdSubPremises.Value;
                    }
                    else
                    if (historyInfo.IdPremises != null)
                    {
                        payment.AddressType = AddressTypes.Premise;
                        payment.IdObject = historyInfo.IdPremises.Value;
                    }
                    else
                    {
                        payment.AddressType = AddressTypes.Building;
                        payment.IdObject = historyInfo.IdBuilding;
                    }
                    payment.FromDate = historyInfo.Date;
                    payment.Payment = Math.Round((historyInfo.K1 + historyInfo.K2 + historyInfo.K3) / 3
                        * historyInfo.Kc * (decimal)historyInfo.RentArea * historyInfo.Hb, 2);
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
                    payment.Payment = rentObject.Payment;
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
                var processes = account.TenancyProcesses;

                List<string> emails = new List<string>();
                foreach (var tp in processes)
                {
                    var curEmails = registryContext.TenancyPersons
                        .Where(per => per.IdProcess == tp.IdProcess && per.Email != null)
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
                .Include(r => r.TenancyProcesses)
                .Include(r => r.Claims)
                .Include(r => r.Charges)
                .Include(r => r.State);
        }

        private IQueryable<KumiAccount> GetQueryIncludes(IQueryable<KumiAccount> query)
        {
            return query.Include(r => r.TenancyProcesses)
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

            var buildingsAssoc = registryContext.TenancyBuildingsAssoc
                .Include(b => b.BuildingNavigation)
                .Include(t => t.ProcessNavigation);

            var premisesAssoc = registryContext.TenancyPremisesAssoc
                .Include(p => p.PremiseNavigation)
                .ThenInclude(b => b.IdBuildingNavigation)
                .Include(t => t.ProcessNavigation);

            var subPremisesAssoc = registryContext.TenancySubPremisesAssoc
                .Include(sp => sp.SubPremiseNavigation)
                .ThenInclude(p => p.IdPremisesNavigation)
                .ThenInclude(b => b.IdBuildingNavigation)
                .Include(t => t.ProcessNavigation);

            IEnumerable<int> idAccounts = new List<int>();
            var filtered = false;

            if (filterOptions.Address.AddressType == AddressTypes.Street || !string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var streets = filterOptions.Address.AddressType == AddressTypes.Street ? addresses : new List<string> { filterOptions.IdStreet };
                var idBuildingAccounts = buildingsAssoc
                    .Where(oba => streets.Contains(oba.BuildingNavigation.IdStreet) && oba.ProcessNavigation.IdAccount != null)
                    .Select(oba => oba.ProcessNavigation.IdAccount.Value);
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => streets.Contains(opa.PremiseNavigation.IdBuildingNavigation.IdStreet) && opa.ProcessNavigation.IdAccount != null)
                    .Select(opa => opa.ProcessNavigation.IdAccount.Value);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => streets.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet) && ospa.ProcessNavigation.IdAccount != null)
                    .Select(ospa => ospa.ProcessNavigation.IdAccount.Value);
                idAccounts = idBuildingAccounts.Union(idPremiseAccounts.Union(idSubPremiseAccounts));
                filtered = true;
            } else
            if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                var idBuildingAccounts = buildingsAssoc
                    .Where(oba => oba.BuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion) && oba.ProcessNavigation.IdAccount != null)
                    .Select(oba => oba.ProcessNavigation.IdAccount.Value);
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion) && opa.ProcessNavigation.IdAccount != null)
                    .Select(opa => opa.ProcessNavigation.IdAccount.Value);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation
                    .IdBuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion) && ospa.ProcessNavigation.IdAccount != null)
                    .Select(ospa => ospa.ProcessNavigation.IdAccount.Value);
                idAccounts = idBuildingAccounts.Union(idPremiseAccounts.Union(idSubPremiseAccounts));
                filtered = true;
            }
            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));

            if ((filterOptions.Address.AddressType == AddressTypes.Building && addressesInt.Any()) || filterOptions.IdBuilding != null)
            {
                if (filterOptions.IdBuilding != null)
                {
                    addressesInt = new List<int> { filterOptions.IdBuilding.Value };
                }
                var idBuildingAccounts = buildingsAssoc
                    .Where(oba => addressesInt.Contains(oba.IdBuilding) && oba.ProcessNavigation.IdAccount != null)
                    .Select(oba => oba.ProcessNavigation.IdAccount.Value);
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdBuilding) && opa.ProcessNavigation.IdAccount != null)
                    .Select(opa => opa.ProcessNavigation.IdAccount.Value);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding) && ospa.ProcessNavigation.IdAccount != null)
                    .Select(ospa => ospa.ProcessNavigation.IdAccount.Value);
                idAccounts = idBuildingAccounts.Union(idPremiseAccounts.Union(idSubPremiseAccounts));
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.Premise && addressesInt.Any()) || filterOptions.IdPremises != null)
            {
                if (filterOptions.IdPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdPremises.Value };
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdPremises) && opa.ProcessNavigation.IdAccount != null)
                    .Select(opa => opa.ProcessNavigation.IdAccount.Value);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises) && ospa.ProcessNavigation.IdAccount != null)
                    .Select(ospa => ospa.ProcessNavigation.IdAccount.Value);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.SubPremise && addressesInt.Any()) || filterOptions.IdSubPremises != null)
            {
                if (filterOptions.IdSubPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdSubPremises.Value };
                }
                idAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdSubPremises) && ospa.ProcessNavigation.IdAccount != null)
                    .Select(ospa => ospa.ProcessNavigation.IdAccount.Value);
                filtered = true;
            }
            if (filtered)
            {
                query = from q in query
                        join idAccount in idAccounts on q.IdAccount equals idAccount
                        select q;
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var idBuildingAccounts = buildingsAssoc
                   .Where(oba => oba.BuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()) && oba.ProcessNavigation.IdAccount != null)
                   .Select(oba => oba.ProcessNavigation.IdAccount.Value);
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant())
                        && opa.ProcessNavigation.IdAccount != null)
                    .Select(opa => opa.ProcessNavigation.IdAccount.Value);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant())
                        && ospa.ProcessNavigation.IdAccount != null)
                    .Select(ospa => ospa.ProcessNavigation.IdAccount.Value);
                query = from q in query
                        join idAccount in idBuildingAccounts.Union(idPremiseAccounts.Union(idSubPremiseAccounts)) on q.IdAccount equals idAccount
                        select q;
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant())
                        && opa.ProcessNavigation.IdAccount != null)
                    .Select(opa => opa.ProcessNavigation.IdAccount.Value);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant())
                        && ospa.ProcessNavigation.IdAccount != null)
                    .Select(ospa => ospa.ProcessNavigation.IdAccount.Value);
                query = from q in query
                        join idAccount in idPremiseAccounts.Union(idSubPremiseAccounts) on q.IdAccount equals idAccount
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
                var tenancyPersons = registryContext.TenancyPersons.Include(tp => tp.IdProcessNavigation)
                    .Where(tp => tp.IdProcessNavigation.IdAccount != null && tp.Surname.Contains(surname));
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

                var idAccounts = tenancyPersons.Select(tp => tp.IdProcessNavigation.IdAccount.Value).Distinct().ToList();

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
            var idAccounts = registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                    .Where(r => r.TenancyPersons.Any(p => p.Email != null) && r.IdAccount != null).Select(r => r.IdAccount.Value);

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
                            where !row.TenancyProcesses.Any()
                            select row;
                    break;
                case 6:
                case 7:
                    var idProcessWithTenants = (from row in registryContext.TenancyPersons
                                               where row.ExcludeDate == null || row.ExcludeDate > DateTime.Now
                                               select row.IdProcess).Distinct();

                    var actualAccountIds = (from row in registryContext.TenancyProcesses
                                            join idProcess in idProcessWithTenants
                                            on row.IdProcess equals idProcess
                                            where (row.RegistrationNum == null || !row.RegistrationNum.EndsWith("н")) && row.IdAccount != null
                                            select row.IdAccount).ToList();
                    if (filterOptions.IdPreset == 6)
                    {
                        // Действующие лицевые счета без действующих наймов
                        query = from row in query
                                where row.TenancyProcesses.Any() && row.IdState == 1 && !actualAccountIds.Contains(row.IdAccount)
                                select row;
                    } else
                    {
                        // Аннулированные лицевые счета с действующими наймами
                        query = from row in query
                                where row.TenancyProcesses.Any() && row.IdState == 2 && actualAccountIds.Contains(row.IdAccount)
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
            var accountTenancyIds = (from account in accounts
                                    join process in registryContext.TenancyProcesses
                                    on account.IdAccount equals process.IdAccount
                                    select new
                                    {
                                        account.IdAccount,
                                        process.IdProcess
                                    }).ToList();
            var tenancyIds = accountTenancyIds.Select(r => r.IdProcess);
            var tenancyProcesses = registryContext.TenancyProcesses.Include(r => r.TenancyRentPeriods)
                .Include(r => r.TenancyPersons).Where(r => tenancyIds.Contains(r.IdProcess)).ToList();

            var buildings = from tbaRow in registryContext.TenancyBuildingsAssoc
                            join buildingRow in registryContext.Buildings
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
                           join premiseRow in registryContext.Premises
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
                              join subPremiseRow in registryContext.SubPremises
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
                                                         paymentRow.RentArea
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
                                                        paymentRow.RentArea
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
                                                           paymentRow.RentArea
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
                        payments.Where(r => r.IdBuilding.ToString() == obj.RentObject.Address.Id && r.IdPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                       Math.Round(paymentsAfter28082019Buildings.Where(
                            r => r.IdBuilding.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.Premise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdPremises.ToString() == obj.RentObject.Address.Id && r.IdSubPremises == null).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019Premises.Where(
                            r => r.IdPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
                if (obj.RentObject.Address.AddressType == AddressTypes.SubPremise)
                {
                    obj.RentObject.Payment =
                        payments.Where(r => r.IdSubPremises.ToString() == obj.RentObject.Address.Id).Sum(r => r.Payment);
                    obj.RentObject.PaymentAfter28082019 =
                        Math.Round(paymentsAfter28082019SubPremises.Where(
                            r => r.IdSubPremises.ToString() == obj.RentObject.Address.Id
                            ).Sum(r => (r.K1 + r.K2 + r.K3) / 3 * r.KC * r.Hb * (decimal)r.RentArea), 2);
                }
            }

            var result = new Dictionary<int, List<KumiAccountTenancyInfoVM>>();
            foreach (var tenancyProcess in tenancyProcesses)
            {
                var idAccount = accountTenancyIds.FirstOrDefault(r => r.IdProcess == tenancyProcess.IdProcess)?.IdAccount;
                if (idAccount == null) continue;
                if (!result.ContainsKey(idAccount.Value))
                    result.Add(idAccount.Value, new List<KumiAccountTenancyInfoVM>());
                var rentObjects = objects.Where(r => r.IdProcess == tenancyProcess.IdProcess).Select(r => r.RentObject).ToList();
                var tenancyInfo = new KumiAccountTenancyInfoVM
                {
                    RentObjects = rentObjects,
                    TenancyProcess = tenancyProcess,
                    Tenant = registryContext.TenancyPersons.FirstOrDefault(r => r.IdProcess == tenancyProcess.IdProcess && r.ExcludeDate == null && r.IdKinship == 1)
                };
                result[idAccount.Value].Add(tenancyInfo);
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
                .Include(r => r.TenancyProcesses)
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
            var tenancyProcesses = account.TenancyProcesses;
            account.Charges = null;
            account.Claims = null;
            account.TenancyProcesses = null;
            account.State = null;
            registryContext.KumiAccounts.Add(account);
            registryContext.SaveChanges();
            UpdateTenancyProcesses(tenancyProcesses, account.IdAccount);
            registryContext.SaveChanges();
        }
        public void Edit(KumiAccount account)
        {
            var tenancyProcesses = account.TenancyProcesses;
            account.Charges = null;
            account.Claims = null;
            account.TenancyProcesses = null;
            account.State = null;
            if (account.IdState == 2)
            {
                account.RecalcMarker = 0;
                account.RecalcReason = null;
            }
            registryContext.KumiAccounts.Update(account);
            UpdateTenancyProcesses(tenancyProcesses, account.IdAccount);
            registryContext.SaveChanges();
        }

        private void UpdateTenancyProcesses(IList<TenancyProcess> tenancyProcesses, int idAccount)
        {
            var oldAccounts = registryContext.TenancyProcesses.Where(r => r.IdAccount == idAccount);
            foreach(var tenancy in oldAccounts)
            {
                tenancy.IdAccount = null;
            }
            foreach (var tenancy in tenancyProcesses)
            {
                var tenancyDb = registryContext.TenancyProcesses.FirstOrDefault(r => r.IdProcess == tenancy.IdProcess);
                if (tenancyDb != null)
                {
                    tenancyDb.IdAccount = idAccount;
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
                var userName = securityService.User.UserName.ToLowerInvariant();
                return registryContext.Executors.FirstOrDefault(e => e.ExecutorLogin != null &&
                                e.ExecutorLogin.ToLowerInvariant() == userName);
            }
        }
    }
}
