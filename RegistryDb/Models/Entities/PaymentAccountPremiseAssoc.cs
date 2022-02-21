namespace RegistryDb.Models.Entities
{
    public class PaymentAccountPremiseAssoc
    {
        public int IdAssoc { get; set; }
        public int IdPremise { get; set; }
        public int IdAccount { get; set; }

        public virtual Premise PremiseNavigation { get; set; }
        public virtual PaymentAccount PaymentAccountNavigation { get; set; }
    }
}