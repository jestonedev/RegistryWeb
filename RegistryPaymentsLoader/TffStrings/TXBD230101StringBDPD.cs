using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXBD230101StringBDPD : TffStringBD
    {
        public TXBD230101StringBDPD(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override KumiPayment ToPayment()
        {
            var paymentST = CredentialString?.ToPaymentST();
            return new KumiPayment
            {
                Guid = tffStringParts[46] == "" ? null : tffStringParts[46],
                IdSource = 2, // BDPD
                NumDocument = tffStringParts[4] == "" ? null : tffStringParts[4],
                DateDocument = TffTypesHelper.StringToDate(tffStringParts[5]),
                DateIn = TffTypesHelper.StringToDate(tffStringParts[8]),
                DateExecute = TffTypesHelper.StringToDate(tffStringParts[9]),
                DatePay = TffTypesHelper.StringToDate(tffStringParts[25]),
                PaymentKind = new KumiPaymentKind {
                    Code = tffStringParts[7]
                },
                OrderPay = TffTypesHelper.StringToInt(tffStringParts[27]),
                OperationType = new KumiOperationType
                {
                    Code = tffStringParts[10]
                },
                Sum = TffTypesHelper.StringToDecimal(tffStringParts[6]) ?? 0,
                Uin = tffStringParts[28] == "" ? null : tffStringParts[28],
                IdPurpose = TffTypesHelper.StringToInt(tffStringParts[26]),
                Purpose = tffStringParts[30] == "" ? null : tffStringParts[30],
                Kbk = paymentST.Kbk, // tffStringParts[32],
                KbkType = paymentST == null ? null : new KumiKbkType
                {
                    Code = paymentST.KbkType
                },
                TargetCode = paymentST == null ? null : paymentST.TargetCode,
                Okato = paymentST == null ? null : paymentST.Okato,
                PaymentReason = new KumiPaymentReason {
                    Code = tffStringParts[34],
                },
                NumDocumentIndicator = tffStringParts[36],
                DateDocumentIndicator = TffTypesHelper.StringToDate(tffStringParts[37]),
                PayerStatus = new KumiPayerStatus
                {
                    Code = tffStringParts[31],
                },
                PayerInn = tffStringParts[11],
                PayerKpp = tffStringParts[12],
                PayerName = tffStringParts[13],
                PayerAccount = tffStringParts[14],
                PayerBankBik = tffStringParts[15],
                PayerBankName = tffStringParts[16],
                PayerBankAccount = tffStringParts[17],
                RecipientInn = tffStringParts[18],
                RecipientKpp = tffStringParts[19],
                RecipientName = tffStringParts[20],
                RecipientAccount = tffStringParts[21],
                RecipientBankBik = tffStringParts[22],
                RecipientBankName = tffStringParts[23],
                RecipientBankAccount = tffStringParts[24],
                DateEnrollUfk = TffTypesHelper.StringToDate(tffStringParts[45]),
            };
        }
    }
}
