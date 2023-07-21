using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXZF210101StringZFPL : TffStringZF
    {
        public TXZF210101StringZFPL(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override KumiPayment ToPayment()
        {
            return new KumiPayment
            {
                Guid = tffStringParts[1],
                IdSource = 5, // ZF_PL
                NumDocument = tffStringParts[3] == "" ? null : tffStringParts[3],
                DateDocument = TffTypesHelper.StringToDate(tffStringParts[4]),
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
                Sum = TffTypesHelper.StringToDecimal(tffStringParts[8]) ?? 0,
                Uin = tffStringParts[11] == "" ? null : tffStringParts[11],
                IdPurpose = null,
                Purpose = tffStringParts[9] == "" ? null : tffStringParts[9],
                Kbk = (tffStringParts[37] == "" ? null : tffStringParts[37]),
                KbkType = null,
                TargetCode = null,
                Okato = null,
                PaymentReason = new KumiPaymentReason
                {
                    Code = tffStringParts[39],
                },
                NumDocumentIndicator = tffStringParts[41],
                DateDocumentIndicator = TffTypesHelper.StringToDate(tffStringParts[43]),
                PayerStatus = new KumiPayerStatus
                {
                    Code = tffStringParts[38],
                },
                PayerInn = tffStringParts[14],
                PayerKpp = tffStringParts[15],
                PayerName = tffStringParts[16],
                PayerAccount = tffStringParts[18],
                PayerBankBik = tffStringParts[20],
                PayerBankName = tffStringParts[19],
                PayerBankAccount = tffStringParts[21],
                RecipientInn = tffStringParts[24],
                RecipientKpp = tffStringParts[25],
                RecipientName = tffStringParts[22],
                RecipientAccount = tffStringParts[29],
                RecipientBankBik = tffStringParts[28],
                RecipientBankName = tffStringParts[27],
                RecipientBankAccount = tffStringParts[26],
            };
        }
    }
}
