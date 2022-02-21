namespace RegistryWeb.Models.Api
{
    public class Roof
    {
        double AreaTotal { get; set; } //Площадь кровли общая, м2
        double AreaPitchedSlate { get; set; } //Площадь кровли шиферная скатная, м2
        double AreaPitchedMetal { get; set; } //Площадь кровли металлическая скатная, м2
        double AreaPitchedOthers { get; set; } //Площадь кровли иная скатная, м2
        double AreaFlat { get; set; } //Площадь кровли плоская, м2
        string LastOverhaulDate { get; set; } //Год проведения последнего капитального ремонта кровли
    }
}
