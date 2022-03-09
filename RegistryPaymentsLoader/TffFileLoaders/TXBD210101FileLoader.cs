﻿using System;
using System.Collections.Generic;
using System.Text;
using RegistryPaymentsLoader.TffStrings;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    class TXBD210101FileLoader : TffFileLoader
    {
        public override string Version => "TXBD210101";

        protected override TffString LoadString(string[] tffStringParts)
        {
            var stringVersion = tffStringParts[0];
            switch(stringVersion)
            {
                case "BDPD":
                    if (tffStringParts.Length != 50) return null;
                    return new TXBD210101StringBDPD(tffStringParts);
                case "BDPDST":
                    if (tffStringParts.Length != 12) return null;
                    return new TXBD210101StringBDPDST(tffStringParts);
                case "BDPL":
                    if (tffStringParts.Length != 45) return null;
                    return new TXBD210101StringBDPL(tffStringParts);
                case "BDPLST":
                    if (tffStringParts.Length != 10) return null;
                    return new TXBD210101StringBDPLST(tffStringParts);
            }
            return null;
        }
    }
}
