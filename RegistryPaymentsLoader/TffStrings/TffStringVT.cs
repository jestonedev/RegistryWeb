using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffStrings
{
    public abstract class TffStringVT: TffString
    {
        public TffStringVT(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public abstract KumiPaymentExtract ToExtract();
    }
}
