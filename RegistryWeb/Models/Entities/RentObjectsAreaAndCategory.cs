using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public partial class RentObjectsAreaAndCategory
    {
        [Column("id_process")]
        public int IdProcess { get; set; }
        [Column("id_building")]
        public int IdBuilding { get; set; }
        [Column("id_premises")]
        public int? IdPremises { get; set; }
        [Column("id_sub_premises")]
        public int? IdSubPremises { get; set; }
        [Column("rent_area")]
        public double RentArea { get; set; }
        [Column("rent_coefficient")]
        public decimal RentCoefficient { get; set; }
        [Column("id_rent_category")]
        public int IdRentCategory { get; set; }
    }
}
