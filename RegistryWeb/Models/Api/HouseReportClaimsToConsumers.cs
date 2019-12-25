namespace RegistryWeb.Models.Api
{
    public class HouseReportClaimsToConsumers
    {
        //sent_claims_count
        int SentClaimsCount { get; set; } //Направлено претензий потребителям- должникам, ед.
        //filed_actions_count
        int FiledActionsCount { get; set; } //Направлено исковых заявлений, ед.
        //received_cash_amount
        double ReceivedCashAmount { get; set; } //Получено денежных средств по результатам претензионно-исковой работы, ед.
    }
}