using System;
using System.Collections.Generic;
using System.Text;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.Helpers;
using RegistryPaymentsLoader.Models;

namespace RegistryPaymentsLoader.TffStrings
{
    public class TXZF210101StringZFZF : TffStringZFZF
    {
        public TXZF210101StringZFZF(string[] tffStringParts) : base(tffStringParts)
        {
        }

        public override ZfPayment GetZfPayment()
        {
            throw new ApplicationException("Неподдерживаемая версия файла ZF");
        }
    }
}
