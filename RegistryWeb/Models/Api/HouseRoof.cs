namespace RegistryWeb.Models.Api
{
    public class HouseRoof
    {
        //id
        int Id { get; set; } //Идентификатор крыши
        //Roof_type
        HouseRoofTypeEnum RoofType { get; set; } //Идентификатор типа крыши 
        //roofing_type
        HouseRoofingTypeEnum RoofingType { get; set; } //Идентификатор типа кровли
    }
}