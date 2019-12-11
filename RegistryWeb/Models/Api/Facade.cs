namespace RegistryWeb.Models.Api
{
    public class Facade
    {
        double AreaTotal { get; set; } //Площадь фасада общая, м2
        double AreaPlastered { get; set; } //Площадь фасада оштукатуренная, м2
        double AreaUnplastered { get; set; } //Площадь фасада неоштукатуренная, м2
        double AreaPanel { get; set; } //Площадь фасада панельная, м2
        double AreaTiled { get; set; } //Площадь фасада, облицованная плиткой, м2
        double AreaLinedSiding { get; set; } //Площадь фасада, облицованная сайдингом, м2
        double AreaWooden { get; set; } //Площадь фасада деревянная, м2
        double AreaInsulatedDecorativePlaster { get; set; } //Площадь утепленного фасада с отделкой декоративной штукатуркой, м2
        double AreaInsulatedTiles { get; set; } //Площадь утепленного фасада с отделкой плиткой, м2
        double Area_insulatedSiding { get; set; } //Площадь утепленного фасада с отделкой сайдингом, м2
        double AreaRiprap { get; set; } //Площадь отмостки, м2
        double AreaGlazingCommonWooden { get; set; } //Площадь остекления мест общего пользования(дерево), м2
        double AreaGlazingCommonPlastic { get; set; } //Площадь остекления мест общего пользования(пластик), м2
        double AreaGlazingIndividualWooden { get; set; } //Площадь индивидуального остекления(дерево), м2
        double AreaDoorFillingsMetal { get; set; } //Площадь металлических дверных заполнений, м2
        double AreaDoorFillingsOthers { get; set; } //Площадь иных дверных заполнений, м2
        string LastOverhaulDate { get; set; } //Год проведения последнего капитального ремонта

    }
}
