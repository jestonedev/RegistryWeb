namespace RegistryPaymentsLoader.Models
{
    public class KumiPaymentST
    {
        public string Kbk { get; set; }
        public string KbkType { get; set; }
        public string TargetCode { get; set; }
        public string Okato { get; set; }
        public decimal Sum { get; set; }
        public int PaymentWay { get; set; }
    }
}