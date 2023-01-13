using System;
using System.Collections.Generic;
using System.Text;
using RegistryPaymentsLoader.TffStrings;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    class TXZF230101FileLoader : TffFileLoader
    {
        public override string Version => "TXZF230101";
        protected override TffString LoadString(string[] tffStringParts)
        {
            var stringVersion = tffStringParts[0];
            switch (stringVersion)
            {
                case "ZF_PP":
                    if (tffStringParts.Length != 45) return null;
                    return new TXZF230101StringZFPP(tffStringParts);
                case "ZF_PL":
                    if (tffStringParts.Length != 46) return null;
                    return new TXZF230101StringZFPL(tffStringParts);
            }
            return null;
        }
    }
}
