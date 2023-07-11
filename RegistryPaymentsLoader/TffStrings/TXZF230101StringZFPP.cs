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
            return new KumiPayment
            {
                Guid = tffStringParts[43] == "" ? null : tffStringParts[43],
                IdSource = 4, // ZF_PP
                NumDocument = tffStringParts[1] == "" ? null : tffStringParts[1],
                DateDocument = TffTypesHelper.StringToDate(tffStringParts[2]),
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
                Sum = TffTypesHelper.StringToDecimal(tffStringParts[3]) ?? 0,
                Uin = tffStringParts[25] == "" ? null : tffStringParts[25],
                IdPurpose = TffTypesHelper.StringToInt(tffStringParts[23]),
                Purpose = tffStringParts[27] == "" ? null : tffStringParts[27],
                Kbk = tffStringParts[29] == "" ? null : tffStringParts[29],
                KbkType = null,
                TargetCode = null,
                Okato = null,
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
                PayerInn = tffStringParts[8],
                PayerKpp = tffStringParts[9],
                PayerName = tffStringParts[10],
                PayerAccount = tffStringParts[11],
                PayerBankBik = tffStringParts[12],
                PayerBankName = tffStringParts[13],
                PayerBankAccount = tffStringParts[14],
                RecipientInn = tffStringParts[15],
                RecipientKpp = tffStringParts[16],
                RecipientName = tffStringParts[17],
                RecipientAccount = tffStringParts[18],
                RecipientBankBik = tffStringParts[19],
                RecipientBankName = tffStringParts[20],
                RecipientBankAccount = tffStringParts[21],
            };
        }
    }
}
