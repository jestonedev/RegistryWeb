namespace RegistryWeb.Models.Api
{
    public enum ServiceStopReasonTypeEnum
    {
        TimeExpired = 1, //Срок действия предоставления услуги истек
        EnabledByMistake //Услуга была включена в список предоставляемых услуг по ошибке
    }
}