using System;

namespace RegistryWeb.Models.Entities
{
    public class PrivAgreement
    {
        public int IdAgreement { get; set; }
        public int IdContract { get; set; }
        public DateTime AgreementDate { get; set; }
        public string AgreementContent { get; set; }
        public string User { get; set; }
        public bool Deleted { get; set; }
    }
}