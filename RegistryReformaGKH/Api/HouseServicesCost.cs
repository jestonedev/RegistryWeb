using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    [XmlRoot]
    public class HouseServicesCost
    {
        [XmlElement(ElementName = "year")]
        public int Year { get; set; } //Год предоставления работы/услуги. Можно указать только «2015»,  «2016» либо «2017», иначе система выдаст ошибку.

        [XmlElement(ElementName = "plan_cost_per_unit", IsNullable = true)]
        public double? PlanCostPerUnit { get; set; } //Годовая плановая стоимость работ (услуг) (руб.)
    }
}