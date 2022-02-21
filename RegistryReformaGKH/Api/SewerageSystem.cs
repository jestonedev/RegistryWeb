namespace RegistryReformaGKH.Api
{
    public class SewerageSystem
    {
        SewerageSystemNameEnum SystemName { get; set; } //Идентификатор типа
        double SystemLength { get; set; } //Длина трубопроводов системы водоотведения, м
        string LastOverhaulDate { get; set; } //Год проведения последнего капитального ремонта системы водоотведения(канализации)

    }
}