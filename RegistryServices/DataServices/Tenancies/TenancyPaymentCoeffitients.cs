using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.DataServices.Tenancies
{
    internal class TenancyPaymentCoeffitients
    {
        internal int IdProcess { get; set; }
        internal int IdObject { get; set; }
        internal decimal Hb { get; set; }
        internal decimal K1 { get; set; }
        internal decimal K2 { get; set; }
        internal decimal K3 { get; set; }
        internal decimal KC { get; set; }
        internal double RentArea { get; set; }

        internal decimal Payment { get => (K1 + K2 + K3) / 3 * KC * Hb * (decimal) RentArea; }
    }
}
