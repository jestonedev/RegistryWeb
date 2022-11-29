using System;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseCommunalServiceCost
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "tariff_start_date", IsNullable = true)]
        public DateTime? TariffStartDate { get; set; } //Дата начала действия тарифа

        [XmlElement(ElementName = "unit_of_measurement", IsNullable = true)]
        public UnitOfMeasureEnum? UnitOfMeasurement { get; set; } //Идентификатор единицы измерения 

        [XmlElement(ElementName = "tariff", IsNullable = true)]
        public double? Tariff { get; set; } //Тариф (цена) (руб.)
    }
}