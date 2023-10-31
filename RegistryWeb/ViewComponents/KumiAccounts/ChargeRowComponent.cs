using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models.Entities.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewComponents.KumiAccounts
{
    public class ChargeRowComponent: ViewComponent
    {
        public IViewComponentResult Invoke(
            IEnumerable<KumiCharge> charges, 
            IEnumerable<KumiChargeCorrection> corrections, 
            KumiCharge charge, int? idAccountCurrent,
            bool hasDgiCharges, bool hasPkkCharges, bool hasPadunCharges,
            DateTime? forecastCalcDate, bool currentPeriodCalced = true, string initTenancyTabClass = null, string initTotalClass = null)
        {
            ViewBag.HasPayments = (charge.PaymentCharges != null && charge.PaymentCharges.Any()) ||
                (charge.DisplayPaymentClaims != null && charge.DisplayPaymentClaims.Any()) ||
                    (charge.UntiedPaymentsInfo != null && charge.UntiedPaymentsInfo.Any());
            ViewBag.Index = charges.TakeWhile(c => c != charge).Count() + 1;
            ViewBag.Count = charges.Where(r => r.Hidden == 0).Count();
            ViewBag.IdAccountCurrent = idAccountCurrent;
            ViewBag.InitTenancyTabClass = initTenancyTabClass;
            ViewBag.InitTotalClass = initTotalClass;

            var currentPaymentCharges = charges.SelectMany(r => r.PaymentCharges).Where(r => r.IdDisplayCharge == charge.IdCharge);
            var currentCorrections = corrections.Where(r => r.Date >= charge.StartDate && r.Date <= charge.EndDate && r.IdAccount == charge.IdAccount);
            var tenancyOnAccount = currentPaymentCharges?.Sum(r => r.TenancyValue) ?? 0m;
            tenancyOnAccount += currentCorrections.Sum(r => r.PaymentTenancyValue);
            var penaltyOnAccount = currentPaymentCharges?.Sum(r => r.PenaltyValue) ?? 0m;
            penaltyOnAccount += currentCorrections.Sum(r => r.PaymentPenaltyValue);

            var dgiOnAccount = currentPaymentCharges?.Sum(r => r.DgiValue) ?? 0m;
            dgiOnAccount += currentCorrections.Sum(r => r.PaymentDgiValue);

            var pkkOnAccount = currentPaymentCharges?.Sum(r => r.PkkValue) ?? 0m;
            pkkOnAccount += currentCorrections.Sum(r => r.PaymentPkkValue);

            var padunOnAccount = currentPaymentCharges?.Sum(r => r.PadunValue) ?? 0m;
            padunOnAccount += currentCorrections.Sum(r => r.PaymentPadunValue);

            var totalOnAccount = currentPaymentCharges?.Sum(r => r.TenancyValue + r.DgiValue + r.PkkValue + r.PadunValue) ?? 0m;
            totalOnAccount += currentCorrections.Sum(r => r.PaymentTenancyValue + r.PaymentDgiValue + r.PaymentPkkValue + r.PaymentPadunValue);

            if (charge.IsBksCharge == 1)
            {
                tenancyOnAccount = charge.PaymentTenancy;
                penaltyOnAccount = charge.PaymentPenalty;
                dgiOnAccount = charge.PaymentDgi;
                pkkOnAccount = charge.PaymentPkk;
                padunOnAccount = charge.PaymentPadun;
                totalOnAccount = charge.PaymentTenancy + charge.PaymentDgi + charge.PaymentPkk + charge.PaymentPadun;
            }
            ViewBag.TenancyOnClaim = charge.PaymentTenancy - tenancyOnAccount;
            ViewBag.PenaltyOnClaim = charge.PaymentPenalty - penaltyOnAccount;
            ViewBag.DgiOnClaim = charge.PaymentDgi - dgiOnAccount;
            ViewBag.PkkOnClaim = charge.PaymentPkk - pkkOnAccount;
            ViewBag.PadunOnClaim = charge.PaymentPadun - padunOnAccount;
            ViewBag.TotalOnClaim = charge.PaymentTenancy + charge.PaymentDgi + charge.PaymentPkk + charge.PaymentPadun - totalOnAccount;

            return View(charge);
        }
    }
}
