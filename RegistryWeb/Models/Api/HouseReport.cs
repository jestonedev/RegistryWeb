namespace RegistryWeb.Models.Api
{
    public class HouseReport
    {
        //common
        HouseReportCommon Common { get; set; } //Отчеты по управлению. Общая информация
        //communal_service
        HouseReportCommunalService CommunalService { get; set; } //Отчеты по управлению. Коммунальные услуги
        //claims_to_consumers
        HouseReportClaimsToConsumers ClaimsToConsumers { get; set; } //Отчеты по управлению. Претензии по качеству работ
        //house_report_quality_of_work_claims
        HouseReportQualityOfWorkClaims HouseReportQualityOfWorkClaims { get; set; } //Отчеты по управлению. Претензионно-исковая работа
    }
}