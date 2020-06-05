using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.SqlViews
{
    public class TenancyPayment
    {
        public int IdProcess { get; set; }
        public int IdBuilding { get; set; }
        public int? IdPremises { get; set; }
        public int? IdSubPremises { get; set; }
        public double RentArea { get; set; }
        public int IdRentCategory { get; set; }
        public decimal Payment { get; set; }
    }
}
