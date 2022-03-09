using RegistryDb.Models.Entities.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffStrings
{
    public abstract class TffStringBD: TffString
    {
        public TffStringBDST CredentialString { get; set; }
        public TffStringBD(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public abstract KumiPayment ToPayment();
    }
}
