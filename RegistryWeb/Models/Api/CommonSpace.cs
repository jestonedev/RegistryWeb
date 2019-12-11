namespace RegistryWeb.Models.Api
{
    public class CommonSpace
    {
        double CommonSpaceArea { get; set; } //Площадь помещений общего пользования, м2
        string CommonSpaceOverhaulDate { get; set; } //Год проведения последнего ремонта помещений общего пользования.Формат поля 4 цифры, например, ‘2015’. 
    }
}
