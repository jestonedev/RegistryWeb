using System;

namespace RegistryDb.Models.Entities
{
    public partial class TenancyPaymentHistory
    {

        public int Id { get; set; }
        
        public int IdBuilding { get; set; }
        public int? IdPremises { get; set; }        
        public int? IdSubPremises { get; set; }
        public double RentArea { get; set; }
        public decimal K1 { get; set; }
        public decimal K2 { get; set; }
        public decimal K3 { get; set; }
        public decimal Kc { get; set; }
        public decimal Hb { get; set; }
        public DateTime Date { get; set; }
        public string Reason { get; set; }
        public virtual Building Building { get; set; }
        public virtual Premise Premise { get; set; }
        public virtual SubPremise SubPremise { get; set; }

        public decimal GetPayment()
        {
            return Math.Round((K1 + K2 + K3) / 3 * Kc * Hb * (decimal)RentArea, 2);
        }
    }
}
