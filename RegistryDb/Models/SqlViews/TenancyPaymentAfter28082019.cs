namespace RegistryDb.Models.SqlViews
{
    public class TenancyPaymentAfter28082019
    {
        public string Key { get; set; }
        public int IdBuilding { get; set; }
        public int? IdPremises { get; set; }
        public int? IdSubPremises { get; set; }
        public double RentArea { get; set; }
        public decimal K1 { get; set; }
        public decimal K2 { get; set; }
        public decimal K3 { get; set; }
        public decimal KC { get; set; }
        public decimal Hb { get; set; }
    }
}
