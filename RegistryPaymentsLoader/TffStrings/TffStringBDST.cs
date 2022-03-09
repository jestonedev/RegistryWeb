using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffStrings
{
    public abstract class TffStringBDST: TffString
    {
        public TffStringBDST(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public abstract KumiPaymentST ToPaymentST();
    }
}
