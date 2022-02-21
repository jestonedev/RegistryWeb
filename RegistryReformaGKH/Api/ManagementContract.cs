using System;
using System.Collections.Generic;

namespace RegistryReformaGKH.Api
{
    public class ManagementContract
    {
        ContractTypeEnum contract_type { get; set; } //Тип договора управления
        DateTime DateStart { get; set; } //Дата начала обслуживания дома по договору управления
        DateTime PlanDateStop { get; set; } //Плановая дата прекращения обслуживания дома по договору управления
        string Jobs { get; set; } //Выполняемые работы
        List<int> JobsFiles { get; set; } //Массив идентификаторов файлов.Если идентификатор файла не указан - это означает удаление файла из анкеты.
        string Responsibility { get; set; }  //Выполнение обязательств
        List<int> ResponsibilityFiles { get; set; } //Массив идентификаторов файлов.Если идентификатор файла не указан - это означает удаление файла из анкеты.
        string Notice { get; set; } //Примечание
        string ServiceCost { get; set; } //Стоимость услуг
        List<int> ServiceCostFiles { get; set; } //Массив идентификаторов файлов.Если идентификатор файла не указан - это означает удаление файла из анкеты.
        string ResourcesTszZsk { get; set; } //Средства ТСЖ или ЖСК
        List<int> ResourcesTszZskFiles { get; set; } //Массив идентификаторов файлов.Если идентификатор файла не указан - это означает удаление файла из анкеты.
        string TermsServiceTszZsk { get; set; } //Условия оказания услуг ТСЖ или ЖСК
        List<int> TermsServiceTszZskFiles { get; set; } //Массив идентификаторов файлов.Если идентификатор файла не указан - это означает удаление файла из анкеты.
    }
}