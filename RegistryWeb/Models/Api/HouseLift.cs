namespace RegistryWeb.Models.Api
{
    public class HouseLift
    {
        //id
        int Id { get; set; }
        //Porch_number
        string PorchNumber { get; set; } //Номер подъезда
        //type
        HouseLiftTypeEnum Type { get; set; } //Идентификатор типа лифта 
        //commissioning_year
        int CommissioningYear { get; set; } //Год ввода в эксплуатацию (Формат: ‘2015’)
    }
}