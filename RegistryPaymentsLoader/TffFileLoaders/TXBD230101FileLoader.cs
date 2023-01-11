using System;
using System.Collections.Generic;
using System.Text;
using RegistryPaymentsLoader.TffStrings;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    class TXBD230101FileLoader : TffFileLoader
    {
        public override string Version => "TXBD230101";
        protected override TffString LoadString(string[] tffStringParts)
        {
            var stringVersion = tffStringParts[0];
            switch (stringVersion)
            {
                case "BDPD":
                    if (tffStringParts.Length != 51) return null;
                    return new TXBD230101StringBDPD(tffStringParts);
                case "BDPDST":
                    if (tffStringParts.Length != 12) return null;
                    return new TXBD230101StringBDPDST(tffStringParts);
                case "BDPL":
                    if (tffStringParts.Length != 46) return null;
                    return new TXBD230101StringBDPL(tffStringParts);
                case "BDPLST":
                    if (tffStringParts.Length != 10) return null;
                    return new TXBD230101StringBDPLST(tffStringParts);
            }
            return null;
        }
    }
}
