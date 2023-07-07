using System;
using System.Collections.Generic;
using System.Text;
using RegistryPaymentsLoader.Helpers;
using RegistryPaymentsLoader.TffStrings;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    class TXVT170101FileLoader : TffFileLoader
    {
        public override string Version => "TXVT170101";
        protected override TffString LoadString(string[] tffStringParts)
        {
            var stringVersion = tffStringParts[0];
            switch (stringVersion)
            {
                case "VT":
                    if (tffStringParts.Length != 26) return null;
                    NoticeDate = TffTypesHelper.StringToDate(tffStringParts[3]);
                    return null;
                case "VTOPER":
                    if (tffStringParts.Length != 19) return null;
                    return new TXVT170101StringVTOPER(tffStringParts);
            }
            return null;
        }
    }
}
