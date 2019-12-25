using System.Collections.Generic;

namespace RegistryWeb.Models.Api
{
    public class HouseServiceReport
    {
        //fact_cost_per_unit
        double FactCostPerUnit { get; set; } //Годовая фактическая стоимость работ (услуг), руб.
        //volumes
        List<HouseServiceReportVolume> Volumes { get; set; } //Детальный перечень выполненных работ (оказанных услуг) в рамках выбранной работы (услуги).
    }
}