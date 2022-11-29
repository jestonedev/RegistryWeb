﻿using RegistryDb.Models.Entities.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.TffStrings
{
    public abstract class TffStringZF: TffString
    {
        public TffStringZF(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public abstract KumiPayment ToPayment();
    }
}
