namespace RegistryWeb.Models.Api
{
    public class HouseCommonProperty
    {
        //id
        int Id { get; set; }
        //name
        string Name { get; set; } //Наименование объекта общего имущества
        //function
        string Function { get; set; } //Назначение объекта общего имущества
        //area
        double Area { get; set; } //Площадь объекта общего имущества (заполняется в отношении помещений и земельных участков), кв.м
        //rent
        HouseCommonPropertyRent Rent { get; set; } //Общее имущество сдается в аренду (в пользование)
    }
}