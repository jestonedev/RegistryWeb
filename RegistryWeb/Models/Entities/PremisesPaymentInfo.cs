using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesPaymentInfo
    {
        public PremisesPaymentInfo()
        {
            Premises = new List<Premise>();
        }

        [Key]
        public int idPremises { get; set; }
        public double payment { get; set; }
        public double CPc { get; set; }
        public double Hb { get; set; }
        public double Kc { get; set; }
        public double K1 { get; set; }
        public double K2 { get; set; }
        public double K3 { get; set; }
        public double rent { get; set; }

        public virtual IList<Premise> Premises { get; set; }
    }
}
