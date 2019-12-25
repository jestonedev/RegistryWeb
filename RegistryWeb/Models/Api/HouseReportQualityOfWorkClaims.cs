namespace RegistryWeb.Models.Api
{
    public class HouseReportQualityOfWorkClaims
    {
        //claims_received_count
        int ClaimsReceivedCount { get; set; } //Количество поступивших претензий по качеству работ, ед.
        //claims_satisfied_count
        int ClaimsSatisfiedCount { get; set; } //Количество удовлетворенных претензий по качеству работ, ед.
        //claims_denied_count
        int ClaimsDeniedCount { get; set; } //Количество претензий по качеству работ, в удовлетворении которых отказано, ед.
        //produced_recalculation_amount
        double ProducedRecalculationAmount { get; set; } //Сумма произведенного перерасчета, руб.
    }
}