using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;

namespace RegistryPaymentsLoader.TffStrings
{
    public class VTBV2String : VTBV1String
    {
        public VTBV2String(string[] tffStringParts) : base(tffStringParts)
        {
        }

        protected override string GenerateConstantGuid()
        {
            var datePart = tffStringParts[3].Replace(".", "").Trim();
            var sumPart = tffStringParts[2].Replace(".", "").Replace(",", "").Trim().PadLeft(12, '0').Substring(0, 12);
            var k1 = tffStringParts[4].PadRight(12, '0').Substring(0, 4);
            var k2 = tffStringParts[4].PadRight(12, '0').Substring(4, 4);
            var k3 = tffStringParts[4].PadRight(12, '0').Substring(8, 4);
            if (NoticeDate == null || NoticeDate < new DateTime(2023, 9, 1))
            {
                k1 = "0000";
                k2 = "0000";
                k3 = "0000";
            }
            return string.Format("{0}-{2}-{3}-{4}-{1}", datePart, sumPart, k1, k2, k3);
        }
    }
}
