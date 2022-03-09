﻿using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXBD210101StringBDPD : TffStringBD
    {
        public TXBD210101StringBDPD(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override KumiPayment ToPayment()
        {
            var paymentST = CredentialString?.ToPaymentST();
            return new KumiPayment
            {
                Guid = tffStringParts[45],
                IdSource = 2, // BDPD
                NumDocument = tffStringParts[4],
                DateDocument = TffTypesHelper.StringToDate(tffStringParts[5]),
                DateIn = TffTypesHelper.StringToDate(tffStringParts[8]),
                DateExecute = TffTypesHelper.StringToDate(tffStringParts[9]),
                DatePay = TffTypesHelper.StringToDate(tffStringParts[25]),
                PaymentKind = new KumiPaymentKind {
                    Name = tffStringParts[7]
                },
                OrderPay = TffTypesHelper.StringToInt(tffStringParts[27]),
                OperationType = new KumiOperationType
                {
                    Name = tffStringParts[10]
                },
                Sum = TffTypesHelper.StringToDecimal(tffStringParts[6]) ?? 0,
                Uin = tffStringParts[28],
                IdPurpose = TffTypesHelper.StringToInt(tffStringParts[26]),
                Purpose = tffStringParts[29],
                Kbk = tffStringParts[31],
                KbkType = paymentST == null ? null : new KumiKbkType
                {
                    Name = paymentST.KbkType
                },
                TargetCode = paymentST == null ? null : paymentST.TargetCode,
                Okato = paymentST == null ? null : paymentST.Okato,
                PaymentReason = new KumiPaymentReason {
                    Name = tffStringParts[33],
                },
                NumDocumentIndicator = tffStringParts[35],
                DateDocumentIndicator = TffTypesHelper.StringToDate(tffStringParts[36]),
                PayerStatus = new KumiPayerStatus
                {
                    Name = tffStringParts[30],
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
            };
        }
    }
}
