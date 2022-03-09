using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;
using RegistryPaymentsLoader.Models;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXVT170101StringVTOPER : TffStringVT
    {
        public TXVT170101StringVTOPER(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override KumiPaymentExtract ToExtract()
        {
            return new KumiPaymentExtract
            {
                Guid = tffStringParts[1],
                CodeDoc = tffStringParts[2],
                NumDoc = tffStringParts[3],
                DateDoc = TffTypesHelper.StringToDate(tffStringParts[4]) ?? DateTime.MinValue,
                SumIn = TffTypesHelper.StringToDecimal(tffStringParts[8]) ?? 0,
                SumZach = TffTypesHelper.StringToDecimal(tffStringParts[10]) ?? 0,
                KbkType = tffStringParts[12],
                Kbk = tffStringParts[13],
                TargetCode = tffStringParts[14],
                Okato = tffStringParts[15],
                InnAdb = tffStringParts[16],
                KppAdb = tffStringParts[17],
            };
        }
    }
}
