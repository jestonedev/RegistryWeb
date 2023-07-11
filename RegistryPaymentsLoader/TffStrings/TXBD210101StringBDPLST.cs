using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;
using RegistryPaymentsLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXBD210101StringBDPLST : TffStringBDST
    {
        public TXBD210101StringBDPLST(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override KumiPaymentST ToPaymentST() {
            return new KumiPaymentST
            {
                Kbk = tffStringParts[1] == "" ? null : tffStringParts[1],
                KbkType = tffStringParts[2] == "" ? null : tffStringParts[2],
                TargetCode = tffStringParts[3] == "" ? null : tffStringParts[3],
                Okato = tffStringParts[5] == "" ? null : tffStringParts[5],
                Sum = TffTypesHelper.StringToDecimal(tffStringParts[6]) ?? 0,
                PaymentWay = TffTypesHelper.StringToInt(tffStringParts[7]) ?? 0
            };
        }
    }
}
