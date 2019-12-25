using System;

namespace RegistryWeb.Models.Api
{
    public class HouseCommunalServiceCost
    {
        //id
        int Id { get; set; }
        //tariff_start_date
        DateTime TariffStartDate { get; set; } //Дата начала действия тарифа
        //unit_of_measurement
        UnitOfMeasureEnum UnitOfMeasurement { get; set; } //Идентификатор единицы измерения 
        //tariff
        double Tariff { get; set; } //Тариф (цена) (руб.)
    }
}