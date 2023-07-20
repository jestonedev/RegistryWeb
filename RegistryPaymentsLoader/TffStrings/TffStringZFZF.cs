using RegistryPaymentsLoader.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffStrings
{
    public abstract class TffStringZFZF: TffString
    {
        public TffStringZFZF(string[] tffStringParts) : base(tffStringParts)
        {
        }
        public abstract ZfPayment GetZfPayment();
    }
}
