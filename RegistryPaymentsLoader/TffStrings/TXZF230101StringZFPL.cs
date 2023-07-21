using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXZF230101StringZFPL : TffStringZF
    {

        public TXZF230101StringZFPL(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override KumiPayment ToPayment()
        {
            var zfPayment = ZfString?.GetZfPayment();
            return new KumiPayment
            {
                Guid = tffStringParts[1] == "" ? null : tffStringParts[1],
                IdSource = 5, // ZF_PL
                NumDocument = zfPayment == null ? (tffStringParts[3] == "" ? null : tffStringParts[3]) : zfPayment.NumDoc,
                DateDocument = zfPayment == null ? TffTypesHelper.StringToDate(tffStringParts[4]) : zfPayment.DateDoc,
                DateIn = TffTypesHelper.StringToDate(tffStringParts[4]),
                DateExecute = TffTypesHelper.StringToDate(tffStringParts[5]),
                DatePay = null,
                PaymentKind = new KumiPaymentKind
                {
                    Code = "0"
                },
                OrderPay = 5,
                OperationType = new KumiOperationType
                {
                    Code = "01"
                },
                Sum = zfPayment == null ? (TffTypesHelper.StringToDecimal(tffStringParts[8]) ?? 0) : zfPayment.Sum,
                Uin = tffStringParts[11] == "" ? null : tffStringParts[11],
                IdPurpose = null,
                Purpose = zfPayment == null ? (tffStringParts[9] == "" ? null : tffStringParts[9]) : zfPayment.Purpose,
                Kbk = zfPayment == null ? null : zfPayment.Kbk,
                KbkType = zfPayment == null ? null : new KumiKbkType { Code = zfPayment.KbkType },
                TargetCode = zfPayment == null ? null : zfPayment.TargetCode,
                Okato = zfPayment == null ? null : zfPayment.Okato,
                PaymentReason = new KumiPaymentReason
                {
                    Code = tffStringParts[40],
                },
                NumDocumentIndicator = tffStringParts[42],
                DateDocumentIndicator = TffTypesHelper.StringToDate(tffStringParts[44]),
                PayerStatus = new KumiPayerStatus
                {
                    Code = tffStringParts[39],
                },
                PayerInn = zfPayment == null ? tffStringParts[15] : zfPayment.PayerInn,
                PayerKpp = zfPayment == null ? tffStringParts[16] : zfPayment.PayerKpp,
                PayerName = zfPayment == null ? tffStringParts[17] : zfPayment.PayerName,
                PayerAccount = tffStringParts[19],
                PayerBankBik = tffStringParts[21],
                PayerBankName = tffStringParts[20],
                PayerBankAccount = tffStringParts[22],
                RecipientInn = zfPayment == null ? tffStringParts[25] : zfPayment.RecipientInn,
                RecipientKpp = zfPayment == null ? tffStringParts[26] : zfPayment.RecipientKpp,
                RecipientName = tffStringParts[23],
                RecipientAccount = tffStringParts[30],
                RecipientBankBik = tffStringParts[29],
                RecipientBankName = tffStringParts[28],
                RecipientBankAccount = tffStringParts[27],
            };
        }
    }
}
