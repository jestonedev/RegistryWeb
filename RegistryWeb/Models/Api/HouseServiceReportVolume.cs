namespace RegistryWeb.Models.Api
{
    public class HouseServiceReportVolume
    {
        //id
        int Id { get; set; }
        //name
        string Name { get; set; } //Наименование работы (услуги), выполняемой в рамках указанного раздела работ (услуг)
        //periodicity
        HouseReportServicesVolumesPeriodicityEnum Periodicity { get; set; } //Периодичность выполнения работ (оказания услуг) 
        //unit_of_measurement
        UnitOfMeasureEnum UnitOfMeasurement { get; set; } //Идентификатор единицы измерения 
        //cost_per_unit
        double CostPerUnit { get; set; } //Стоимость на единицу измерения, руб.  
    }
}