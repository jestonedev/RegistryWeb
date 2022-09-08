using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class BKSV1String : TffStringBD
    {
        public BKSV1String(string[] tffStringParts) : base(tffStringParts)
        {
        }

        private string GenerateConstantGuid()
        {
            var datePart = tffStringParts[5].Replace(".", "").Trim();
            var sumPart = tffStringParts[3].Replace(".", "").Replace(",", "").Trim().PadLeft(12, '0').Substring(0, 12);
            var accountPart = tffStringParts[0].Trim().PadLeft(12, '0').Substring(0, 12);
            return string.Format("{0}-{1}-{2}-{3}-{4}", datePart,
                accountPart.Substring(0, 4), accountPart.Substring(4, 4), accountPart.Substring(8, 4),
                sumPart);
        }

        private string GetPurpose()
        {
            var account = tffStringParts[0].Trim();
            var snp = tffStringParts[2];
            var address = tffStringParts[1];
            var uo = tffStringParts[4];
            return string.Format("ЛИЦЕВОЙ СЧЕТ: {0};{1};{2};{3}", account, snp, address, uo);
        }

        public override KumiPayment ToPayment()
        {
            return new KumiPayment
            {
                Guid = GenerateConstantGuid(),
                IdSource = 6, // BKS
                DateDocument = TffTypesHelper.StringToDate(tffStringParts[5]),
                DateIn = TffTypesHelper.StringToDate(tffStringParts[5]),
                DateExecute = TffTypesHelper.StringToDate(tffStringParts[5]),
                DatePay = TffTypesHelper.StringToDate(tffStringParts[5]),
                PaymentKind = new KumiPaymentKind {
                    Code = "0"
                },
                OrderPay = 5,
                OperationType = new KumiOperationType
                {
                    Code = "01"
                },
                Sum = TffTypesHelper.StringToDecimal(tffStringParts[3]) ?? 0,
                Purpose = GetPurpose(),
                Kbk = "90111109044041000120",
                PayerName = string.Format("{0}", tffStringParts[2])
            };
        }
    }
}
