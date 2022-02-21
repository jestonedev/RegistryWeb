using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class TotalAreaAvgCost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите цену 1 кв. м. жилья")]
        [Range(0, Double.MaxValue, ErrorMessage = "Цена 1 кв. м. жилья должна быть больше нуля")]
        public decimal Cost { get; set; }
    }
}
