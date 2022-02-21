using System.Collections.Generic;
using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseServiceReport
    {
        [XmlElement(ElementName = "fact_cost_per_unit", IsNullable = true)]
        public double? FactCostPerUnit { get; set; } //Годовая фактическая стоимость работ (услуг), руб.

        [XmlArray(ElementName = "volumes", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<HouseServiceReportVolume> Volumes { get; set; } //Детальный перечень выполненных работ (оказанных услуг) в рамках выбранной работы (услуги).
    }
}