using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class VTBV1String : TffStringBD
    {
        public DateTime? NoticeDate { get; set; }
        public VTBV1String(string[] tffStringParts) : base(tffStringParts)
        {
        }

        protected virtual string GenerateConstantGuid()
        {
            var datePart = tffStringParts[3].Replace(".", "").Trim();
            var sumPart = tffStringParts[2].Replace(".", "").Replace(",", "").Trim().PadLeft(12, '0').Substring(0, 12);
            return string.Format("{0}-0000-0000-0000-{1}", datePart, sumPart);
        }

        private string GetPurpose()
        {
            var tenant = tffStringParts[0].Trim();
            var address = tffStringParts[1];
            return string.Format("{0};{1}", tenant, address);
        }

        public override KumiPayment ToPayment()
        {
            return new KumiPayment
            {
                Guid = GenerateConstantGuid(),
                IdSource = 7, // VTB
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
                PayerName = string.Format("{0}", tffStringParts[0]),
                DateEnrollUfk = NoticeDate
            };
        }
    }
}
