using System;
using System.Collections.Generic;
using System.Text;
using RegistryPaymentsLoader.TffStrings;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    class TXZF210101FileLoader : TffFileLoader
    {
        public override string Version => "TXZF210101";
        protected override TffString LoadString(string[] tffStringParts)
        {
            var stringVersion = tffStringParts[0];
            switch (stringVersion)
            {
                case "ZF_PP":
                    if (tffStringParts.Length != 44) return null;
                    return new TXZF210101StringZFPP(tffStringParts);
                case "ZF_PL":
                    if (tffStringParts.Length != 45) return null;
                    return new TXZF210101StringZFPL(tffStringParts);
            }
            return null;
        }
    }
}
