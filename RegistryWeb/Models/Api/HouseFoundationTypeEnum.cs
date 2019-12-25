namespace RegistryWeb.Models.Api
{
    public enum HouseFoundationTypeEnum
    {
        Tape = 1, //Ленточный
        ConcretePillars, //Бетонные столбы
        Pile, //Свайный
        Other, //Иной
        Columnar, //Столбчатый
        Solid, //Сплошной
        Prefabricated, //Сборный
        Missing //Отсутствует
    }
}