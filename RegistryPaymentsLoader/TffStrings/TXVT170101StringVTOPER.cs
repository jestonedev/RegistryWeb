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
        private readonly DateTime? noticeDate;

        public TXVT170101StringVTOPER(string[] tffStringParts, DateTime? noticeDate) : base(tffStringParts)
        {
            this.noticeDate = noticeDate;
        }

        public override KumiPaymentExtract ToExtract()
        {
            return new KumiPaymentExtract
            {
                Guid = tffStringParts[1] == "" ? null : tffStringParts[1],
                CodeDoc = tffStringParts[2] == "" ? null : tffStringParts[2],
                NumDoc = tffStringParts[3] == "" ? null : tffStringParts[3],
                DateDoc = TffTypesHelper.StringToDate(tffStringParts[4]) ?? DateTime.MinValue,
                CodeDocAdp = tffStringParts[5] == "" ? null : tffStringParts[5],
                SumIn = TffTypesHelper.StringToDecimal(tffStringParts[8]) ?? 0,
                SumOut = TffTypesHelper.StringToDecimal(tffStringParts[9]) ?? 0,
                SumZach = TffTypesHelper.StringToDecimal(tffStringParts[10]) ?? 0,
                KbkType = tffStringParts[12] == "" ? null : tffStringParts[12],
                Kbk = tffStringParts[13] == "" ? null : tffStringParts[13],
                TargetCode = tffStringParts[14] == "" ? null : tffStringParts[14],
                Okato = tffStringParts[15] == "" ? null : tffStringParts[15],
                InnAdb = tffStringParts[16] == "" ? null : tffStringParts[16],
                KppAdb = tffStringParts[17] == "" ? null : tffStringParts[17],
                DateEnrollUfk = noticeDate
            };
        }
    }
}
