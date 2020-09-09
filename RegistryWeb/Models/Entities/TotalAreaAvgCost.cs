using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public partial class TotalAreaAvgCost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите цену 1 кв. м. жилья")]
        [Range(0, Double.MaxValue, ErrorMessage = "Цена 1 кв. м. жилья должна быть больше нуля")]
        public decimal Cost { get; set; }
    }
}
