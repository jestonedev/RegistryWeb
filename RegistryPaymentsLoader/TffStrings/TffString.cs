using RegistryDb.Models.Entities.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffStrings
{
    public abstract class TffString
    {
        protected readonly string[] tffStringParts;

        public TffString(string[] tffStringParts) {
            this.tffStringParts = tffStringParts;
        }
    }
}
