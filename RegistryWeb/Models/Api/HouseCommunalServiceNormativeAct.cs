using System;

namespace RegistryWeb.Models.Api
{
    public class HouseCommunalServiceNormativeAct
    {
        //id
        int Id { get; set; }
        //document_date
        DateTime DocumentDate { get; set; } //Дата нормативного правового акта, устанавливающего норматив потребления коммунальной услуги
        //document_number
        string DocumentNumber { get; set; } //Номер нормативного правового акта, устанавливающего норматив потребления коммунальной услуги
        //document_organization_name
        string DocumentOrganizationName { get; set; } //Наименование принявшего акт органа
    }
}