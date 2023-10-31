using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models.Entities.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.ViewComponents.KumiAccounts
{
    public class ChargesTotalComponent: ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<KumiCharge> charges, IEnumerable<KumiChargeCorrection> corrections,
            int? idAccount, bool hasDgiCharges, bool hasPkkCharges, bool hasPadunCharges,
            DateTime? forecastCalcDate, bool currentPeriodCalced = true)
        {
            var totalChargeTenancy = 0.00m;
            var totalChargePenalty = 0.00m;
            var totalPaymentTenancy = 0.00m;
            var totalPaymentPenalty = 0.00m;
            var totalPaymentTenancyByAccount = 0.00m;
            var totalPaymentPenaltyByAccount = 0.00m;
            var totalRecalcTenancy = 0.00m;
            var totalRecalcPenalty = 0.00m;

            var totalChargeDgi = 0.00m;
            var totalPaymentDgi = 0.00m;
            var totalPaymentDgiByAccount = 0.00m;
            var totalRecalcDgi = 0.00m;

            var totalChargePkk = 0.00m;
            var totalPaymentPkk = 0.00m;
            var totalPaymentPkkByAccount = 0.00m;
            var totalRecalcPkk = 0.00m;

            var totalChargePadun = 0.00m;
            var totalPaymentPadun = 0.00m;
            var totalPaymentPadunByAccount = 0.00m;
            var totalRecalcPadun = 0.00m;

            var totalPaymentTotalByAccount = 0.00m;

            foreach(var charge in charges)
            {
                if (forecastCalcDate == null || charge.EndDate != forecastCalcDate || currentPeriodCalced)
                {
                    totalChargeTenancy += charge.ChargeTenancy;
                    totalChargePenalty += charge.ChargePenalty;
                    totalChargeDgi += charge.ChargeDgi;
                    totalChargePkk += charge.ChargePkk;
                    totalChargePadun += charge.ChargePadun;

                    totalRecalcTenancy += charge.RecalcTenancy + charge.CorrectionTenancy;
                    totalRecalcPenalty += charge.RecalcPenalty + charge.CorrectionPenalty;
                    totalRecalcDgi += charge.RecalcDgi + charge.CorrectionDgi;
                    totalRecalcPkk += charge.RecalcPkk + charge.CorrectionPkk;
                    totalRecalcPadun += charge.RecalcPadun + charge.CorrectionPadun;
                }
                totalPaymentTenancy += charge.PaymentTenancy;
                totalPaymentPenalty += charge.PaymentPenalty;
                totalPaymentDgi += charge.PaymentDgi;
                totalPaymentPkk += charge.PaymentPkk;
                totalPaymentPadun += charge.PaymentPadun;

                var paymentCharges = charges.SelectMany(r => r.PaymentCharges).Where(r => r.IdDisplayCharge == charge.IdCharge);
                var filteredCorrections = corrections.Where(r => r.Date >= charge.StartDate && r.Date <= charge.EndDate);

                var tenancyOnAccount = paymentCharges?.Sum(r => r.TenancyValue) ?? 0m;
                tenancyOnAccount += filteredCorrections.Sum(r => r.PaymentTenancyValue);
                if (charge.IsBksCharge == 1)
                {
                    tenancyOnAccount = charge.PaymentTenancy;
                }
                var penaltyOnAccount = paymentCharges?.Sum(r => r.PenaltyValue) ?? 0m;
                penaltyOnAccount += filteredCorrections.Sum(r => r.PaymentPenaltyValue);
                if (charge.IsBksCharge == 1)
                {
                    penaltyOnAccount = charge.PaymentPenalty;
                }
                totalPaymentTenancyByAccount += tenancyOnAccount;
                totalPaymentPenaltyByAccount += penaltyOnAccount;

                var dgiOnAccount = paymentCharges?.Sum(r => r.DgiValue) ?? 0m;
                dgiOnAccount += filteredCorrections.Sum(r => r.PaymentDgiValue);
                if (charge.IsBksCharge == 1)
                {
                    dgiOnAccount = charge.PaymentDgi;
                }
                totalPaymentDgiByAccount += dgiOnAccount;

                var pkkOnAccount = paymentCharges?.Sum(r => r.PkkValue) ?? 0m;
                pkkOnAccount += filteredCorrections.Sum(r => r.PaymentPkkValue);
                if (charge.IsBksCharge == 1)
                {
                    pkkOnAccount = charge.PaymentPkk;
                }
                totalPaymentPkkByAccount += pkkOnAccount;

                var padunOnAccount = paymentCharges?.Sum(r => r.PadunValue) ?? 0m;
                padunOnAccount += filteredCorrections.Sum(r => r.PaymentPadunValue);
                if (charge.IsBksCharge == 1)
                {
                    padunOnAccount = charge.PaymentPadun;
                }
                totalPaymentPadunByAccount += padunOnAccount;

                var totalOnAccount = paymentCharges?.Sum(r => r.TenancyValue + r.DgiValue + r.PkkValue + r.PadunValue) ?? 0m;
                totalOnAccount += filteredCorrections.Sum(r => r.PaymentTenancyValue + r.PaymentDgiValue + r.PaymentPkkValue + r.PaymentPadunValue);
                if (charge.IsBksCharge == 1)
                {
                    totalOnAccount = charge.PaymentTenancy + charge.PaymentDgi + charge.PaymentPkk + charge.PaymentPadun;
                }
                totalPaymentTotalByAccount += totalOnAccount;
            }

            ViewBag.TotalChargeTenancy = totalChargeTenancy;
            ViewBag.TotalChargePenalty = totalChargePenalty;
            ViewBag.TotalPaymentTenancy = totalPaymentTenancy;
            ViewBag.TotalPaymentTenancyByAccount = totalPaymentTenancyByAccount;
            ViewBag.TotalPaymentPenalty = totalPaymentPenalty;
            ViewBag.TotalPaymentPenaltyByAccount = totalPaymentPenaltyByAccount;
            ViewBag.TotalRecalcTenancy = totalRecalcTenancy;
            ViewBag.TotalRecalcPenalty = totalRecalcPenalty;

            ViewBag.TotalChargeDgi = totalChargeDgi;
            ViewBag.TotalPaymentDgi = totalPaymentDgi;
            ViewBag.TotalPaymentDgiByAccount = totalPaymentDgiByAccount;
            ViewBag.TotalRecalcDgi = totalRecalcDgi;


            ViewBag.TotalChargePkk = totalChargePkk;
            ViewBag.TotalPaymentPkk = totalPaymentPkk;
            ViewBag.TotalPaymentPkkByAccount = totalPaymentPkkByAccount;
            ViewBag.TotalRecalcPkk = totalRecalcPkk;

            ViewBag.TotalChargePadun = totalChargePadun;
            ViewBag.TotalPaymentPadun = totalPaymentPadun;
            ViewBag.TotalPaymentPadunByAccount = totalPaymentPadunByAccount;
            ViewBag.TotalRecalcPadun = totalRecalcPadun;

            ViewBag.TotalPaymentTotal = totalPaymentTenancy + totalPaymentDgi + totalPaymentPkk + totalPaymentPadun;
            ViewBag.TotalPaymentTotalByAccount = totalPaymentTotalByAccount;

            ViewBag.HasDgiCharges = hasDgiCharges;
            ViewBag.HasPkkCharges = hasPkkCharges;
            ViewBag.HasPadunCharges = hasPadunCharges;
            ViewBag.IdAccount = idAccount;

            return View();
        }
    }
}
