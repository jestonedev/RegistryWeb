using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    [XmlRoot]
    public class HouseServiceReportVolume
    {
        [XmlElement(ElementName = "id", IsNullable = true)]
        public int? Id { get; set; }

        [XmlElement(ElementName = "name", IsNullable = true)]
        public string Name { get; set; } //Наименование работы (услуги), выполняемой в рамках указанного раздела работ (услуг)

        [XmlElement(ElementName = "periodicity", IsNullable = true)]
        public HouseReportServicesVolumesPeriodicityEnum? Periodicity { get; set; } //Периодичность выполнения работ (оказания услуг) 

        [XmlElement(ElementName = "unit_of_measurement", IsNullable = true)]
        public UnitOfMeasureEnum? UnitOfMeasurement { get; set; } //Идентификатор единицы измерения 

        [XmlElement(ElementName = "cost_per_unit", IsNullable = true)]
        public double? CostPerUnit { get; set; } //Стоимость на единицу измерения, руб.  
    }
}