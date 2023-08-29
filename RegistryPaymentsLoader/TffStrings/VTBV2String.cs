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
    }
}
