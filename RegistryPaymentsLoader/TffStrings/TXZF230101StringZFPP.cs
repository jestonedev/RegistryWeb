using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXZF230101StringZFPP : TffStringZF
    {
        public TXZF230101StringZFPP(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override KumiPayment ToPayment()
        {
            var zfPayment = ZfString?.GetZfPayment();
            return new KumiPayment
            {
                Guid = tffStringParts[43] == "" ? null : tffStringParts[43],
                IdSource = 4, // ZF_PP
                NumDocument = zfPayment == null ? (tffStringParts[1] == "" ? null : tffStringParts[1]) : zfPayment.NumDoc,
                DateDocument = zfPayment == null ? TffTypesHelper.StringToDate(tffStringParts[2]) : zfPayment.DateDoc,
                DateIn = TffTypesHelper.StringToDate(tffStringParts[5]),
                DateExecute = TffTypesHelper.StringToDate(tffStringParts[6]),
                DatePay = TffTypesHelper.StringToDate(tffStringParts[22]),
                PaymentKind = new KumiPaymentKind
                {
                    Code = tffStringParts[4]
                },
                OrderPay = TffTypesHelper.StringToInt(tffStringParts[24]),
                OperationType = new KumiOperationType
                {
                    Code = tffStringParts[7]
                },
                Sum = zfPayment == null ? (TffTypesHelper.StringToDecimal(tffStringParts[3]) ?? 0) : zfPayment.Sum,
                Uin = tffStringParts[25] == "" ? null : tffStringParts[25],
                IdPurpose = TffTypesHelper.StringToInt(tffStringParts[23]),
                Purpose = zfPayment == null ? (tffStringParts[27] == "" ? null : tffStringParts[27]) : zfPayment.Purpose,
                Kbk = zfPayment == null ? null : zfPayment.Kbk,
                KbkType = zfPayment == null ? null : new KumiKbkType { Code = zfPayment.KbkType },
                TargetCode = zfPayment == null ? null : zfPayment.TargetCode,
                Okato = zfPayment == null ? null : zfPayment.Okato,
                PaymentReason = new KumiPaymentReason
                {
                    Code = tffStringParts[31],
                },
                NumDocumentIndicator = tffStringParts[33],
                DateDocumentIndicator = TffTypesHelper.StringToDate(tffStringParts[34]),
                PayerStatus = new KumiPayerStatus
                {
                    Code = tffStringParts[28],
                },
                PayerInn = zfPayment == null ? tffStringParts[8] : zfPayment.PayerInn,
                PayerKpp = zfPayment == null ? tffStringParts[9] : zfPayment.PayerKpp,
                PayerName = zfPayment == null ? tffStringParts[10] : zfPayment.PayerName,
                PayerAccount = tffStringParts[11],
                PayerBankBik = tffStringParts[12],
                PayerBankName = tffStringParts[13],
                PayerBankAccount = tffStringParts[14],
                RecipientInn = zfPayment == null ? tffStringParts[15] : zfPayment.RecipientInn,
                RecipientKpp = zfPayment == null ? tffStringParts[16] : zfPayment.RecipientKpp,
                RecipientName = tffStringParts[17],
                RecipientAccount = tffStringParts[18],
                RecipientBankBik = tffStringParts[19],
                RecipientBankName = tffStringParts[20],
                RecipientBankAccount = tffStringParts[21],
            };
        }
    }
}
