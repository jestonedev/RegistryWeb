using RegistryDb.Models;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.Classes;
using RegistryServices.Enums;
using RegistryServices.Models.KumiPayments;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using RegistryServices.DataServices.KumiAccounts;

namespace RegistryServices.DataServices.KumiPayments
{
    public class KumiPaymentsDistributionsService
    {
        private readonly RegistryContext registryContext;
        private readonly KumiAccountsCalculationService calculationService;

        public KumiPaymentsDistributionsService(
            RegistryContext registryContext, KumiAccountsCalculationService calculationService)
        {
            this.registryContext = registryContext;
            this.calculationService = calculationService;
        }

        internal List<KumiPaymentDistributionInfoToObject> GetDistributionInfoToObjects(List<int> idPayments)
        {
            var consolidatedPaymentsIds = registryContext.KumiPayments.Where(r => idPayments.Contains(r.IdPayment) && r.IsConsolidated == 1).Select(r => r.IdPayment).ToList();
            var childPayments = registryContext.KumiPayments
                .Where(r => r.IdParentPayment != null && consolidatedPaymentsIds.Contains(r.IdParentPayment.Value)).ToList();
            var consolidatedPayments = new Dictionary<int, List<KumiPayment>>();
            if (childPayments.Any())
            {
                foreach (var group in childPayments.GroupBy(r => r.IdParentPayment.Value))
                {
                    consolidatedPayments.Add(group.Key, group.Select(r => r).ToList());
                    var subChildPayments = group.ToList();
                    while (true)
                    {
                        var newParentIds = subChildPayments.Select(r => r.IdPayment);
                        subChildPayments = registryContext.KumiPayments.Where(r => r.IdParentPayment != null &&
                            newParentIds.Contains(r.IdParentPayment.Value)).ToList();
                        if (!subChildPayments.Any())
                            break;
                        consolidatedPayments[group.Key].AddRange(subChildPayments);
                    }
                }
            }

            var idPaymentsAll = idPayments.Union(consolidatedPayments.SelectMany(r => r.Value).Select(r => r.IdPayment)).ToList();

            var kpc = (from kpcRow in registryContext.KumiPaymentCharges
                       where idPaymentsAll.Contains(kpcRow.IdPayment)
                       select kpcRow).ToList();

            var cIds = kpc.Select(r => r.IdCharge);

            var c = (from cRow in registryContext.KumiCharges
                     where cIds.Contains(cRow.IdCharge)
                     select cRow).ToList();

            var aIds = c.Select(r => r.IdAccount);

            var a = (from aRow in registryContext.KumiAccounts
                     where aIds.Contains(aRow.IdAccount)
                     select aRow).ToList();

            var charges = (from kpcRow in kpc
                           join cRow in c
                           on kpcRow.IdCharge equals cRow.IdCharge
                           join aRow in a
                           on cRow.IdAccount equals aRow.IdAccount
                           select new
                           {
                               kpcRow,
                               cRow,
                               aRow
                           }).ToList();

            var tenants = (from assocRow in registryContext.KumiAccountsTenancyProcessesAssocs
                           join tRow in registryContext.TenancyProcesses
                           on assocRow.IdProcess equals tRow.IdProcess
                           join tpRow in registryContext.TenancyPersons
                           on tRow.IdProcess equals tpRow.IdProcess
                           where aIds.Contains(assocRow.IdAccount) && tpRow.IdKinship == 1 && tpRow.ExcludeDate == null
                           select new
                           {
                               assocRow.IdAccount,
                               tRow.IdProcess,
                               Tenant = string.Concat(tpRow.Surname, ' ', tpRow.Name, ' ', tpRow.Patronymic)
                           }).ToList();

            var paymentChargesInfo = charges.Select(r => new KumiPaymentDistributionInfoToAccount
            {
                ObjectType = KumiPaymentDistributeToEnum.ToKumiAccount,
                IdPayment = r.kpcRow.IdPayment,
                IdAccount = r.cRow.IdAccount,
                IdCharge = r.kpcRow.IdDisplayCharge ?? r.kpcRow.IdCharge,
                Account = r.aRow.Account,
                DistrubutedToPenaltySum = r.kpcRow.PenaltyValue,
                DistrubutedToTenancySum = r.kpcRow.TenancyValue,
                DistrubutedToDgiSum = r.kpcRow.DgiValue,
                DistrubutedToPkkSum = r.kpcRow.PkkValue,
                DistrubutedToPadunSum = r.kpcRow.PadunValue,
                Sum = r.kpcRow.PenaltyValue + r.kpcRow.TenancyValue + r.kpcRow.DgiValue + r.kpcRow.PkkValue + r.kpcRow.PadunValue,
                Tenant = tenants.Where(t => t.IdAccount == r.cRow.IdAccount).OrderByDescending(t => t.IdProcess)
                                 .Select(t => t.Tenant).FirstOrDefault()
            }).ToList();

            var kpcClaims = (from kpcRow in registryContext.KumiPaymentClaims
                             where idPaymentsAll.Contains(kpcRow.IdPayment)
                             select kpcRow).ToList();

            cIds = kpcClaims.Select(r => r.IdClaim);

            var cClaims = (from cRow in registryContext.Claims
                           where cIds.Contains(cRow.IdClaim)
                           select cRow).ToList();

            aIds = cClaims.Where(r => r.IdAccountKumi != null).Select(r => (int)r.IdAccountKumi);

            var aClaims = (from aRow in registryContext.KumiAccounts
                           where aIds.Contains(aRow.IdAccount)
                           select aRow).ToList();

            var claims = (from kpcRow in kpcClaims
                          join cRow in cClaims
                          on kpcRow.IdClaim equals cRow.IdClaim
                          join aRow in aClaims
                          on cRow.IdAccountKumi equals aRow.IdAccount
                          select new
                          {
                              kpcRow,
                              cRow,
                              aRow
                          }).ToList();

            var tenantsClaims = (from assocRow in registryContext.KumiAccountsTenancyProcessesAssocs
                                 join tRow in registryContext.TenancyProcesses
                                 on assocRow.IdProcess equals tRow.IdProcess
                                 join tpRow in registryContext.TenancyPersons
                                 on tRow.IdProcess equals tpRow.IdProcess
                                 where aIds.Contains(assocRow.IdAccount) && tpRow.IdKinship == 1 && tpRow.ExcludeDate == null
                                 select new
                                 {
                                     assocRow.IdAccount,
                                     tRow.IdProcess,
                                     Tenant = string.Concat(tpRow.Surname, ' ', tpRow.Name, ' ', tpRow.Patronymic)
                                 }).ToList();

            var paymentClaimsInfo = claims.Select(r => new KumiPaymentDistributionInfoToClaim
            {
                ObjectType = KumiPaymentDistributeToEnum.ToClaim,
                IdPayment = r.kpcRow.IdPayment,
                IdClaim = r.cRow.IdClaim,
                IdCharge = r.kpcRow.IdDisplayCharge ?? 0,
                IdAccountKumi = r.cRow.IdAccountKumi,
                Account = r.aRow.Account,
                DistrubutedToPenaltySum = r.kpcRow.PenaltyValue,
                DistrubutedToTenancySum = r.kpcRow.TenancyValue,
                DistrubutedToDgiSum = r.kpcRow.DgiValue,
                DistrubutedToPkkSum = r.kpcRow.PkkValue,
                DistrubutedToPadunSum = r.kpcRow.PadunValue,
                Sum = r.kpcRow.PenaltyValue + r.kpcRow.TenancyValue + r.kpcRow.DgiValue + r.kpcRow.PkkValue + r.kpcRow.PadunValue,
                Tenant = tenantsClaims.Where(t => t.IdAccount == r.cRow.IdAccountKumi).OrderByDescending(t => t.IdProcess)
                                .Select(t => t.Tenant).FirstOrDefault()
            }).ToList();

            foreach (var consolidatedPayment in consolidatedPayments)
            {
                var childPaymentsIds = consolidatedPayment.Value.Select(r => r.IdPayment).ToList();
                var comparedPaymentChargesInfo = paymentChargesInfo.Where(r => childPaymentsIds.Contains(r.IdPayment)).ToList();
                foreach (var paymentChargeInfo in comparedPaymentChargesInfo)
                {
                    paymentChargesInfo.Add(new KumiPaymentDistributionInfoToAccount
                    {
                        ObjectType = paymentChargeInfo.ObjectType,
                        IdPayment = consolidatedPayment.Key,
                        IdAccount = paymentChargeInfo.IdAccount,
                        IdCharge = paymentChargeInfo.IdCharge,
                        Account = paymentChargeInfo.Account,
                        DistrubutedToPenaltySum = paymentChargeInfo.DistrubutedToPenaltySum,
                        DistrubutedToTenancySum = paymentChargeInfo.DistrubutedToTenancySum,
                        DistrubutedToDgiSum = paymentChargeInfo.DistrubutedToDgiSum,
                        DistrubutedToPkkSum = paymentChargeInfo.DistrubutedToPkkSum,
                        DistrubutedToPadunSum = paymentChargeInfo.DistrubutedToPadunSum,
                        Sum = paymentChargeInfo.Sum,
                        Tenant = paymentChargeInfo.Tenant
                    });
                }

                var comparedPaymentClaimsInfo = paymentClaimsInfo.Where(r => childPaymentsIds.Contains(r.IdPayment)).ToList();
                foreach (var paymentClaimInfo in comparedPaymentClaimsInfo)
                {
                    paymentClaimsInfo.Add(new KumiPaymentDistributionInfoToClaim
                    {
                        ObjectType = paymentClaimInfo.ObjectType,
                        IdPayment = consolidatedPayment.Key,
                        IdClaim = paymentClaimInfo.IdClaim,
                        IdCharge = paymentClaimInfo.IdCharge,
                        IdAccountKumi = paymentClaimInfo.IdAccountKumi,
                        Account = paymentClaimInfo.Account,
                        DistrubutedToPenaltySum = paymentClaimInfo.DistrubutedToPenaltySum,
                        DistrubutedToTenancySum = paymentClaimInfo.DistrubutedToTenancySum,
                        DistrubutedToDgiSum = paymentClaimInfo.DistrubutedToDgiSum,
                        DistrubutedToPkkSum = paymentClaimInfo.DistrubutedToPkkSum,
                        DistrubutedToPadunSum = paymentClaimInfo.DistrubutedToPadunSum,
                        Sum = paymentClaimInfo.Sum,
                        Tenant = paymentClaimInfo.Tenant
                    });
                }
            }

            return paymentChargesInfo.Select(r => (KumiPaymentDistributionInfoToObject)r).Union(paymentClaimsInfo).ToList();
        }

        internal List<RegistryTuple<KumiPayment, KumiAccount>> AutoDistributeUploadedPayments(List<KumiPayment> insertedPayments)
        {
            var filteredPaymentsInfo = AutoDistributeFilterPayments(insertedPayments);
            var accountsInfo = new Dictionary<int, KumiAccount>();
            foreach (var paymentInfo in filteredPaymentsInfo)
            {
                var account = (from aRow in registryContext.KumiAccounts.Include(r => r.Charges).AsNoTracking()
                               where aRow.Account == paymentInfo.Key.Item1
                               select aRow).FirstOrDefault();
                if (account != null) accountsInfo.Add(paymentInfo.Value.IdPayment, account);
            }
            var accountIds = accountsInfo.Select(r => r.Value.IdAccount).ToList();
            var tenantsInfo = registryContext.GetTenantsByAccountIds(accountIds);
            var result = new List<RegistryTuple<KumiPayment, KumiAccount>>();
            foreach (var paymentInfo in filteredPaymentsInfo)
            {
                var accounts = accountsInfo.Where(r => r.Key == paymentInfo.Value.IdPayment);
                if (!accounts.Any()) continue;
                var account = accounts.First();
                var tenantInfo = tenantsInfo.FirstOrDefault(r => r.IdAccount == account.Value.IdAccount);
                var tenant = string.IsNullOrWhiteSpace(account.Value.Owner) ? tenantInfo?.Tenant : account.Value.Owner;
                if (tenant == null) continue;
                var paymentTenant = (paymentInfo.Key.Item2 ?? "").Split("$$", 2)[0];
                var paymentTenantParts = paymentTenant.ToLowerInvariant().Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var tenantParts = tenant.ToLowerInvariant().Split(" ");
                if (paymentTenantParts.Length != tenantParts.Length) continue;
                var tenantEqual = true;
                for (var i = 0; i < paymentTenantParts.Length; i++)
                {
                    var initial = paymentTenantParts[i];
                    if (!(paymentTenantParts[i] == tenantParts[i] ||
                        (initial.Length == 1 && initial == tenantParts[i].Substring(0, 1))))
                        tenantEqual = false;
                }
                if (!tenantEqual) continue;

                var chargeTenancy = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalanceTenancy - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargeTenancy ?? 0)) : account.Value.CurrentBalanceTenancy ?? 0;
                var chargePenalty = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalancePenalty - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargePenalty ?? 0)) : account.Value.CurrentBalancePenalty ?? 0;
                var chargeDgi = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalanceDgi - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargeDgi ?? 0)) : account.Value.CurrentBalanceDgi ?? 0;
                var chargePkk = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalancePkk - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargePkk ?? 0)) : account.Value.CurrentBalancePkk ?? 0;
                var chargePadun = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalancePadun - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargePadun ?? 0)) : account.Value.CurrentBalancePadun ?? 0;

                try
                {
                    if (paymentInfo.Value.Kbk == "90111109044041000120" && paymentInfo.Value.Sum == chargeTenancy + chargePenalty + chargePkk + chargePadun)
                    {
                        DistributePaymentToAccount(paymentInfo.Value.IdPayment, account.Value.IdAccount, KumiPaymentDistributeToEnum.ToKumiAccount,
                        chargeTenancy ?? 0, chargePenalty ?? 0, 0, chargePkk ?? 0, chargePadun ?? 0);
                    }
                    else

                    if (paymentInfo.Value.Kbk == "90111705040041111180" && paymentInfo.Value.Sum == chargeDgi)
                    {
                        DistributePaymentToAccount(paymentInfo.Value.IdPayment, account.Value.IdAccount,
                            KumiPaymentDistributeToEnum.ToKumiAccount, 0, 0, chargeDgi ?? 0, 0, 0);
                    }
                    else continue;

                }
                catch
                {
                    continue;
                }
                result.Add(new RegistryTuple<KumiPayment, KumiAccount> { Item1 = paymentInfo.Value, Item2 = account.Value });
            }
            return result;
        }

        private Dictionary<Tuple<string, string>, KumiPayment> AutoDistributeFilterPayments(List<KumiPayment> insertedPayments)
        {
            var result = new Dictionary<Tuple<string, string>, KumiPayment>();
            var i = 0;
            foreach (var payment in insertedPayments)
            {
                if (!new[] { "90111109044041000120", "90111705040041111180" }.Contains(payment.Kbk)) continue;
                if (payment.Sum == 0) continue;
                var purpose = payment.Purpose;
                if (string.IsNullOrEmpty(purpose)) continue;
                var courtOrderRegex = new Regex(@"(ИД)[ ]*([0-9][-][0-9]{1,6}[ ]?[\/][ ]?([0-9]{4}|[0-9]{2}))");
                if (courtOrderRegex.IsMatch(purpose)) continue;

                var tenant = (string)null;
                var account = (string)null;
                var accountRegex = new Regex(@"(ЛИЦ(\.?|ЕВОЙ)?[ ]+СЧЕТ|ЛС)[ ]*[:]?[ ]*([0-9]{6,})");
                var accountMatch = accountRegex.Match(purpose);
                if (accountMatch.Success)
                    account = accountMatch.Groups[3].Value;

                var payerRegExp1 = new Regex(@"ФИО_ПЛАТЕЛЬЩИКА:([а-яА-Я ]+)");
                if (payment.IdSource == 6)
                    tenant = payment.PayerName;
                else
                if (new Regex(@"\/\/без НДС$").IsMatch(purpose))
                {
                    var purposeParts = purpose.Split("//");
                    if (purposeParts.Length > 3)
                        tenant = purposeParts[purposeParts.Length - 3];
                }
                else
                if (payerRegExp1.IsMatch(purpose))
                {
                    var payerMatch = payerRegExp1.Match(purpose);
                    tenant = payerMatch.Groups[1].Value;
                }
                else
                if (payment.PayerName != null && new Regex("^[а-яА-Я]+[ ][а-яА-Я]+([ ][а-яА-Я]+)?$").IsMatch(payment.PayerName))
                {
                    tenant = payment.PayerName;
                }
                if (account == null || tenant == null) continue;
                result.Add(new Tuple<string, string>(account, tenant + "$$" + i.ToString()), payment);
                i++;
            }
            return result;
        }

        public KumiPaymentDistributionInfo CancelDistributePaymentToAccount(int idPayment, List<int> idClaims, List<int> idAccounts)
        {
            var payment = registryContext.KumiPayments.Include(r => r.PaymentClaims).Include(r => r.PaymentCharges).FirstOrDefault(r => r.IdPayment == idPayment);
            if (payment == null)
                throw new ApplicationException("Не найдена платеж в базе данных");

            var idClaimsLocal = idClaims.ToList();
            List<int> idAccountsLocal =
                idAccounts.Union(
                registryContext.Claims.Where(r => r.IdAccountKumi != null && idClaimsLocal.Contains(r.IdClaim)).Select(r => r.IdAccountKumi.Value)).Distinct().ToList();

            IQueryable<KumiAccount> accounts = registryContext.KumiAccounts.Where(r => idAccountsLocal.Contains(r.IdAccount));

            foreach (var account in accounts)
            {
                if (account.IdState == 2)
                    throw new ApplicationException("Нельзя отменить распределение, т.к. связанный лицевой счет аннулирован");
            }

            foreach (var paymentClaim in payment.PaymentClaims.GroupBy(r => r.IdClaim))
            {
                if (!idClaims.Contains(paymentClaim.Key)) continue;
                var claim = registryContext.Claims.FirstOrDefault(r => r.IdClaim == paymentClaim.Key);
                if (claim == null)
                    throw new ApplicationException(
                        string.Format("Произошла ошибка во время отмены распределения платежа. Не найдена исковая работа с идентификатором {0}", claim.IdClaim));

                claim.AmountTenancyRecovered = (claim.AmountTenancyRecovered ?? 0) - paymentClaim.Select(r => r.TenancyValue).Sum();
                claim.AmountPenaltiesRecovered = (claim.AmountPenaltiesRecovered ?? 0) - paymentClaim.Select(r => r.PenaltyValue).Sum();
                claim.AmountDgiRecovered = (claim.AmountDgiRecovered ?? 0) - paymentClaim.Select(r => r.DgiValue).Sum();
                claim.AmountPkkRecovered = (claim.AmountPkkRecovered ?? 0) - paymentClaim.Select(r => r.PkkValue).Sum();
                claim.AmountPadunRecovered = (claim.AmountPadunRecovered ?? 0) - paymentClaim.Select(r => r.PadunValue).Sum();
                registryContext.Claims.Update(claim);
            }

            foreach (var paymentClaim in payment.PaymentClaims)
            {
                if (!idClaims.Contains(paymentClaim.IdClaim)) continue;
                registryContext.KumiPaymentClaims.Remove(paymentClaim);
            }

            foreach (var paymentCharge in payment.PaymentCharges)
            {
                var charge = registryContext.KumiCharges.Where(r => r.IdCharge == paymentCharge.IdCharge).FirstOrDefault();
                if (charge == null || !idAccounts.Contains(charge.IdAccount)) continue;
                registryContext.KumiPaymentCharges.Remove(paymentCharge);
            }

            payment.IsPosted = 0;

            var parentPayment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == payment.IdParentPayment);
            KumiPayment consolidatedParentPayment = null;
            while (parentPayment != null)
            {
                if (parentPayment.IsConsolidated != 0)
                {
                    consolidatedParentPayment = parentPayment;
                    break;
                }
                else
                if (parentPayment.IdParentPayment != null)
                    parentPayment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == parentPayment.IdParentPayment);
                else
                    break;
            }
            if (consolidatedParentPayment != null)
            {
                consolidatedParentPayment.IsPosted = 0;
                registryContext.KumiPayments.Update(consolidatedParentPayment);
            }

            registryContext.SaveChanges();
            registryContext.DetachAllEntities();

            // Recalculate
            var startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day + 1);


            var endCalcDate = DateTime.Now.Date;
            endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);

            if (DateTime.Now.Date.Day >= 25) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.AddMonths(1);
                endCalcDate = endCalcDate.AddDays(1).AddMonths(1).AddDays(-1);
            }

            calculationService.RecalculateAccounts(accounts, startRewriteDate, endCalcDate, false);

            return new KumiPaymentDistributionInfo
            {
                IdPayment = idPayment,
                Sum = payment.Sum,
                DistrubutedToTenancySum = 0,
                DistrubutedToPenaltySum = 0,
                DistrubutedToDgiSum = 0,
                DistrubutedToPkkSum = 0,
                DistrubutedToPadunSum = 0
            };
        }

        public KumiPaymentDistributionInfo DistributePaymentToAccount(int idPayment, int idObject, KumiPaymentDistributeToEnum distributeTo,
            decimal tenancySum, decimal penaltySum, decimal dgiSum, decimal pkkSum, decimal padunSum)
        {
            var payment = registryContext.KumiPayments.Include(r => r.PaymentClaims).Include(r => r.PaymentCharges).FirstOrDefault(r => r.IdPayment == idPayment);
            if (payment == null)
                throw new ApplicationException("Не найдена платеж в базе данных");

            if (payment.IsConsolidated != 0)
                throw new ApplicationException("Платеж помечен как сводное платежное поручение и не может быть распределен. Вместо него необходимо распределять детализирующие платежи");

            if (tenancySum + penaltySum + dgiSum + pkkSum + padunSum == 0)
                throw new ApplicationException("Не указана распределяемая сумма");

            if (payment.Sum > 0 && tenancySum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на найм. Указание отрицательной суммы разрешено только для возвратов");

            if (payment.Sum > 0 && penaltySum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на пени. Указание отрицательной суммы разрешено только для возвратов");

            if (payment.Sum > 0 && dgiSum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на ДГИ. Указание отрицательной суммы разрешено только для возвратов");

            if (payment.Sum > 0 && pkkSum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на ПКК. Указание отрицательной суммы разрешено только для возвратов");

            if (payment.Sum > 0 && padunSum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на Падун. Указание отрицательной суммы разрешено только для возвратов");

            if (!new[] { "90111109044041000120", "90111705040041111180" }.Contains(payment.Kbk))
                throw new ApplicationException(string.Format("Нельзя распределить платеж с КБК {0}. Допускаются только платежи с КБК 90111109044041000120 (плата за наем) и 90111705040041111180 (возмещение ДГИ)", payment.Kbk));

            var distributedTenancySum = payment.PaymentCharges.Select(r => r.TenancyValue).Sum() +
                payment.PaymentClaims.Select(r => r.TenancyValue).Sum();
            var distributedPenaltySum = payment.PaymentCharges.Select(r => r.PenaltyValue).Sum() +
                payment.PaymentClaims.Select(r => r.PenaltyValue).Sum();
            var distributedDgiSum = payment.PaymentCharges.Select(r => r.DgiValue).Sum() +
                payment.PaymentClaims.Select(r => r.DgiValue).Sum();
            var distributedPkkSum = payment.PaymentCharges.Select(r => r.PkkValue).Sum() +
                payment.PaymentClaims.Select(r => r.PkkValue).Sum();
            var distributedPadunSum = payment.PaymentCharges.Select(r => r.PadunValue).Sum() +
                payment.PaymentClaims.Select(r => r.PadunValue).Sum();

            if (Math.Abs(payment.Sum) < Math.Abs(distributedTenancySum + distributedPenaltySum + distributedDgiSum + distributedPkkSum + distributedPadunSum
                + tenancySum + penaltySum + dgiSum + pkkSum + padunSum))
                throw new ApplicationException(string.Format("Распределяемая сумма {0} превышает остаток по платежу {1}",
                    tenancySum + penaltySum + dgiSum + pkkSum + padunSum,
                    payment.Sum - distributedTenancySum - distributedPenaltySum - distributedDgiSum - distributedPkkSum - distributedPadunSum));

            if (payment.Kbk == "90111705040041111180" && (tenancySum != 0 || penaltySum != 0 || pkkSum != 0 || padunSum != 0))
                throw new ApplicationException("Платежи с КБК 90111705040041111180 (возмещение ДГИ) можно распределять только на задолженность ДГИ");

            if (payment.Kbk == "90111109044041000120" && (dgiSum != 0))
                throw new ApplicationException("Платежи с КБК 90111109044041000120 (плата за наем) нельзя распределять на задолженность ДГИ");

            IQueryable<KumiAccount> accounts = null;

            switch (distributeTo)
            {
                case KumiPaymentDistributeToEnum.ToKumiAccount:
                    accounts = registryContext.KumiAccounts.Where(r => r.IdAccount == idObject);
                    if (accounts.Count() == 0)
                        throw new ApplicationException(string.Format("Не найден лицевой счет с реестровым номером {0}", idObject));

                    var account = accounts.First();
                    if (accounts.First().IdState == 2)
                        throw new ApplicationException(string.Format("Нельзя распределить платеж на аннулированный лицевой счет {0}", account.Account));

                    var date = DateTime.Now.Date;
                    var startPeriodDate = date.AddDays(-date.Day + 1);
                    var endPeriodDate = date.AddDays(-date.Day + 1).AddMonths(1).AddDays(-1);
                    if (date.Day >= 25)
                    {
                        startPeriodDate = startPeriodDate.AddMonths(1);
                        endPeriodDate = endPeriodDate.AddDays(1).AddMonths(1).AddDays(-1);
                    }
                    var charge = registryContext.KumiCharges.AsNoTracking()
                        .FirstOrDefault(r => r.IdAccount == idObject && r.StartDate == startPeriodDate && r.EndDate == endPeriodDate);

                    var paymentCharge = new KumiPaymentCharge
                    {
                        IdPayment = idPayment,
                        TenancyValue = tenancySum,
                        PenaltyValue = penaltySum,
                        DgiValue = dgiSum,
                        PkkValue = pkkSum,
                        PadunValue = padunSum,
                        Date = DateTime.Now.Date
                    };

                    if (charge == null)
                    {
                        charge = new KumiCharge
                        {
                            IdAccount = idObject,
                            StartDate = startPeriodDate,
                            EndDate = endPeriodDate,
                            PaymentTenancy = tenancySum,
                            PaymentPenalty = penaltySum,
                            PaymentDgi = dgiSum,
                            PaymentPkk = pkkSum,
                            PaymentPadun = padunSum,
                            Hidden = 1
                        };
                        paymentCharge.Charge = charge;
                        registryContext.KumiCharges.Add(charge);
                    }
                    else
                    {
                        paymentCharge.IdCharge = charge.IdCharge;
                    }
                    registryContext.KumiPaymentCharges.Add(paymentCharge);
                    break;
                case KumiPaymentDistributeToEnum.ToClaim:
                    var claim = registryContext.Claims.Include(r => r.ClaimStates).FirstOrDefault(r => r.IdClaim == idObject);
                    var idAccount = claim.IdAccountKumi;
                    if (claim == null)
                        throw new ApplicationException(string.Format("Не найдена исковая работа с реестровым номером {0}", idObject));
                    if (claim.ClaimStates.Any())
                    {
                        var lastState = claim.ClaimStates.Last();
                        if (lastState.IdStateType == 6)
                            throw new ApplicationException(string.Format("Нельзя распределить платеж на завершенную исковую работу", idObject));
                    }
                    if (idAccount == null)
                        throw new ApplicationException(string.Format("Исковая работа {0} не привязана к лицевому счету КУМИ", idObject));
                    accounts = registryContext.KumiAccounts.Where(r => r.IdAccount == idAccount);

                    if (accounts.Count() == 0)
                        throw new ApplicationException(string.Format("Не найден лицевой счет с реестровым номером {0}", idAccount));

                    account = accounts.First();
                    if (accounts.First().IdState == 2)
                        throw new ApplicationException(string.Format("Нельзя распределить платеж на исковую работу, привязанную к аннулированному лицевому счету {0}", account.Account));

                    claim.AmountTenancyRecovered = (claim.AmountTenancyRecovered ?? 0) + tenancySum;
                    claim.AmountPenaltiesRecovered = (claim.AmountPenaltiesRecovered ?? 0) + penaltySum;
                    claim.AmountDgiRecovered = (claim.AmountDgiRecovered ?? 0) + dgiSum;
                    claim.AmountPkkRecovered = (claim.AmountPkkRecovered ?? 0) + pkkSum;
                    claim.AmountPadunRecovered = (claim.AmountPadunRecovered ?? 0) + padunSum;
                    registryContext.Claims.Update(claim);

                    registryContext.KumiPaymentClaims.Add(new KumiPaymentClaim
                    {
                        IdPayment = idPayment,
                        IdClaim = idObject,
                        TenancyValue = tenancySum,
                        PenaltyValue = penaltySum,
                        DgiValue = dgiSum,
                        PkkValue = pkkSum,
                        PadunValue = padunSum,
                        Date = DateTime.Now.Date
                    });
                    break;
            }

            if (payment.Sum == distributedTenancySum + distributedPenaltySum + distributedDgiSum + distributedPkkSum + distributedPadunSum
                + tenancySum + penaltySum + dgiSum + pkkSum + padunSum)
            {
                payment.IsPosted = 1;
                registryContext.KumiPayments.Update(payment);

                if (payment.IdParentPayment != null)
                {
                    var parentPayment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == payment.IdParentPayment);
                    KumiPayment consolidatedParentPayment = null;
                    while (parentPayment != null)
                    {
                        if (parentPayment.IsConsolidated != 0)
                        {
                            consolidatedParentPayment = parentPayment;
                            break;
                        }
                        else
                        if (parentPayment.IdParentPayment != null)
                            parentPayment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == parentPayment.IdParentPayment);
                        else
                            break;
                    }
                    if (consolidatedParentPayment != null)
                    {
                        var childPayments = registryContext.KumiPayments.Where(r => r.IdParentPayment == consolidatedParentPayment.IdPayment).ToList();
                        var resultChildPayments = new List<KumiPayment>();
                        while (true)
                        {
                            if (!childPayments.Any())
                                break;
                            resultChildPayments.AddRange(childPayments);
                            var childPaymentsIds = childPayments.Select(r => r.IdPayment).ToList();
                            childPayments = registryContext.KumiPayments.Where(r => r.IdParentPayment != null &&
                                childPaymentsIds.Contains(r.IdParentPayment.Value)).ToList();
                        }
                        var childPaymentsSum = resultChildPayments.Where(r => r.IsPosted == 1).Select(r => r.Sum).Sum();
                        if (consolidatedParentPayment.Sum == childPaymentsSum)
                        {
                            consolidatedParentPayment.IsPosted = 1;
                            registryContext.KumiPayments.Update(consolidatedParentPayment);
                        }
                    }
                }
            }

            registryContext.SaveChanges();

            registryContext.DetachAllEntities();

            // Recalculate
            var startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day + 1);


            var endCalcDate = DateTime.Now.Date;
            endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);

            if (DateTime.Now.Date.Day >= 25) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.AddMonths(1);
                endCalcDate = endCalcDate.AddDays(1).AddMonths(1).AddDays(-1);
            }

            calculationService.RecalculateAccounts(accounts, startRewriteDate, endCalcDate, false);

            return new KumiPaymentDistributionInfo
            {
                IdPayment = idPayment,
                Sum = payment.Sum,
                DistrubutedToTenancySum = distributedTenancySum + tenancySum,
                DistrubutedToPenaltySum = distributedPenaltySum + penaltySum,
                DistrubutedToDgiSum = distributedDgiSum + dgiSum,
                DistrubutedToPkkSum = distributedPkkSum + pkkSum,
                DistrubutedToPadunSum = distributedPadunSum + padunSum
            };
        }

    }
}
