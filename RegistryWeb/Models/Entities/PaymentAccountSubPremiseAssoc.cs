namespace RegistryWeb.Models.Entities
{
    public class PaymentAccountSubPremiseAssoc
    {
        public int IdAssoc { get; set; }
        public int IdSubPremise { get; set; }
        public int IdAccount { get; set; }
        public virtual SubPremise SubPremiseNavigation { get; set; }
        public virtual PaymentAccount PaymentAccountNavigation { get; set; }
    }
}