using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class BKSV2String : TffStringBD
    {
        public BKSV2String(string[] tffStringParts) : base(tffStringParts)
        {
        }

        private string GenerateConstantGuid()
        {
            var datePart = tffStringParts[3].Replace(".", "").Trim();
            var sumPart = tffStringParts[2].Replace(".", "").Replace(",", "").Trim().PadLeft(12, '0').Substring(0, 12);
            var accountPart = tffStringParts[0].Trim().PadLeft(12, '0').Substring(0, 12);
            return string.Format("{0}-{1}-{2}-{3}-{4}", datePart,
                accountPart.Substring(0, 4), accountPart.Substring(4, 4), accountPart.Substring(8, 4),
                sumPart);
        }

        private string GetPurpose()
        {
            var account = tffStringParts[0].Trim();
            var address = tffStringParts[1];
            return string.Format("ЛИЦЕВОЙ СЧЕТ: {0};{1}", account, address);
        }

        public override KumiPayment ToPayment()
        {
            return new KumiPayment
            {
                Guid = GenerateConstantGuid(),
                IdSource = 6, // BKS
                DateDocument = TffTypesHelper.StringToDate(tffStringParts[3]),
                DateIn = TffTypesHelper.StringToDate(tffStringParts[3]),
                DateExecute = TffTypesHelper.StringToDate(tffStringParts[3]),
                DatePay = TffTypesHelper.StringToDate(tffStringParts[3]),
                PaymentKind = new KumiPaymentKind {
                    Code = "0"
                },
                OrderPay = 5,
                OperationType = new KumiOperationType
                {
                    Code = "01"
                },
                Sum = TffTypesHelper.StringToDecimal(tffStringParts[2]) ?? 0,
                Purpose = GetPurpose(),
                Kbk = "90111109044041000120",
                DateEnrollUfk = TffTypesHelper.StringToDate(tffStringParts[4])
            };
        }
    }
}
