using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;
using RegistryPaymentsLoader.Models;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXZF230101StringZFZF : TffStringZFZF
    {
        public TXZF230101StringZFZF(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override ZfPayment GetZfPayment()
        {
            return new ZfPayment
            {
                PayerName = tffStringParts[13] == "" ? null : tffStringParts[13],
                PayerInn = tffStringParts[14] == "" ? null : tffStringParts[14],
                PayerKpp = tffStringParts[15] == "" ? null : tffStringParts[15],
                NumDoc = tffStringParts[22] == "" ? null : tffStringParts[22],
                DateDoc = TffTypesHelper.StringToDate(tffStringParts[23]),
                RecipientInn = tffStringParts[24] == "" ? null : tffStringParts[24],
                RecipientKpp = tffStringParts[25] == "" ? null : tffStringParts[25],
                Kbk = tffStringParts[26] == "" ? null : tffStringParts[26],
                KbkType = tffStringParts[27] == "" ? null : tffStringParts[27],
                TargetCode = tffStringParts[28] == "" ? null : tffStringParts[28],
                Okato = tffStringParts[29] == "" ? null : tffStringParts[29],
                Sum = TffTypesHelper.StringToDecimal(tffStringParts[30]) ?? 0,
                Purpose = tffStringParts[31] == "" ? null : tffStringParts[31],
            };
        }
    }
}
