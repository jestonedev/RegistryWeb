using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXBD230101StringBDPL : TffStringBD
    {
        public TXBD230101StringBDPL(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override KumiPayment ToPayment()
        {
            var paymentST = CredentialString?.ToPaymentST();
            return new KumiPayment
            {
                Guid = tffStringParts[1] == "" ? null : tffStringParts[1],
                IdSource = 3, // BDPL
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
                Kbk = paymentST.Kbk, // tffStringParts[37],
                KbkType = paymentST == null ? null : new KumiKbkType
                {
                    Code = paymentST.KbkType
                },
                TargetCode = paymentST == null ? null : paymentST.TargetCode,
                Okato = paymentST == null ? null : paymentST.Okato,
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
                PayerInn = tffStringParts[15],
                PayerKpp = tffStringParts[16],
                PayerName = tffStringParts[17],
                PayerAccount = tffStringParts[19],
                PayerBankBik = tffStringParts[21],
                PayerBankName = tffStringParts[20],
                PayerBankAccount = tffStringParts[22],
                RecipientInn = tffStringParts[25],
                RecipientKpp = tffStringParts[26],
                RecipientName = tffStringParts[23],
                RecipientAccount = tffStringParts[30],
                RecipientBankBik = tffStringParts[29],
                RecipientBankName = tffStringParts[28],
                RecipientBankAccount = tffStringParts[27],
                DateEnrollUfk = TffTypesHelper.StringToDate(tffStringParts[5])
            };
        }
    }
}
