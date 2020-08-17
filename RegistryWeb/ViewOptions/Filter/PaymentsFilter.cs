using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class PaymentsFilter : FilterAddressOptions
    {
        public string Account { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public int? IdSubPremises { get; set; }
        public int? IdPremises { get; set; }
        public int? IdPreset { get; set; } // Поисковые пресеты
        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty() && IdPremises == null && IdSubPremises == null;
        }

        public bool IsModalEmpty()
        {
            return Account == null &&
                IdStreet == null && House == null && PremisesNum == null && IdPreset == null;
        }
    }
}
