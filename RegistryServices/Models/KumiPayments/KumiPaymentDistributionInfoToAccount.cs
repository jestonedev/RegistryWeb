namespace RegistryServices.Models.KumiPayments
{
    public class KumiPaymentDistributionInfoToAccount: KumiPaymentDistributionInfoToObject
    {
        public int IdAccount { get; set; }
        public string Account { get; set; }
        public string Tenant { get; set; }
    }
}
