namespace RegistryWeb.Models.Api
{
    public class HouseServicesCost
    {
        //year
        int Year { get; set; } //Год предоставления работы/услуги. Можно указать только «2015»,  «2016» либо «2017», иначе система выдаст ошибку.
        //plan_cost_per_unit
        double PlanCostPerUnit { get; set; } //Годовая плановая стоимость работ (услуг) (руб.)
    }
}