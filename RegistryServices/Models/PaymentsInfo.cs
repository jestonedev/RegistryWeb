using RegistryWeb.Enums;

namespace RegistryWeb.ViewModel
{
    public class PaymentsInfo
    {
        public int IdObject { get; set; }
        public AddressTypes AddresType { get; set; }
        public decimal Payment { get; set; }
        public decimal PaymentAfter28082019 { get; set; }
        public decimal Nb { get; set; }
        public decimal KC { get; set; }
        public decimal K1 { get; set; }
        public decimal K2 { get; set; }
        public decimal K3 { get; set; }
    }
}
