using RegistryDb.Models;
using RegistryDb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class PaymentsAccountTableVM
    {
        public IEnumerable<Payment> Payments { get; set; }
        public Dictionary<int, List<Address>> RentObjects { get; set; }
        public Payment LastPayment { get; set; }
        public PaymentAccountTableJson PaymentAccountTableJson { get; set; }
    }
}
