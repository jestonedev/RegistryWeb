namespace RegistryServices.Models.KumiPayments
{
    public class KumiPaymentDistributionInfoToClaim: KumiPaymentDistributionInfoToObject
    {
        public int IdClaim { get; set; }
        public int? IdAccountKumi { get; set; }
        public string Account { get; set; }
        public string Tenant { get; set; }
    }
}
