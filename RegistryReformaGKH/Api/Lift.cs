using System;
namespace RegistryReformaGKH.Api
{
    public class Lift
    {
        string porch_number { get; set; } //номер подъезда
        string factory_number { get; set; } //заводской номер
        int stops_count { get; set; } //количество остановок
        int capacity { get; set; } //грузоподъемность, кг
        string date_exploitation { get; set; } //год ввода в эксплуатацию
        string date_last_repair { get; set; } //год проведения последнего капремонта
        DateTime plan_period { get; set; } //плановый срок замены(вывода из эксплуатации)
        string manufacturer { get; set; } //изготовитель
    }
}