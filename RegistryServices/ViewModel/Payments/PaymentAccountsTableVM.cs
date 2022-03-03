using RegistryDb.Models.Entities;
using RegistryWeb.ViewModel;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Payments
{
    public class PaymentsAccountTableVM
    {
        public IEnumerable<Payment> Payments { get; set; }
        public Dictionary<int, List<Address>> RentObjects { get; set; }
        public Payment LastPayment { get; set; }
        public PaymentAccountTableJson PaymentAccountTableJson { get; set; }
    }
}
