namespace RegistryReformaGKH.Api
{
    public class Basement
    {
        BasementStageEnum BasementStage { get; set; } //Сведения о подвале
        double BasementArea { get; set; } //Площадь подвальных помещений(включая помещения подвала и техподполье, если оно требует ремонта), м2
        string BasementLastOverhaulDate { get; set; } //Год проведения последнего капитального ремонта подвальных помещений
    }
}
