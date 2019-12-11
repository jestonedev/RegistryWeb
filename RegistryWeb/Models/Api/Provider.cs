namespace RegistryWeb.Models.Api
{
    public class Provider
    {
        string Inn { get; set; } //ИНН
        string Alias { get; set; } //Наименование поставщика
        string AdditionalInfo { get; set; } //Дополнительная информация
        bool SuppliedViaManagementOrganization { get; set; } //Поставляется через управляющую организацию
        bool IsSupported { get; set; } //Услуга не предоставляется
    }
}