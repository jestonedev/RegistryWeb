namespace RegistryWeb.Models.Api
{
    public enum AlarmReasonEnum
    {
        PhysicalWear = 1, //Физический износ
        EnvironmentalInfluence, //Влияние окружающей среды
        NaturalDisaster, //Природные катастрофы
        TechnogenicCharacter, //Причины техногенного характера
        Fire, //Пожар
        Other //Иная
    }
}