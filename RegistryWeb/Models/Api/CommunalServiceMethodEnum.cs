namespace RegistryWeb.Models.Api
{
    public enum CommunalServiceMethodEnum
    {
        ManagementContract = 1, //Предоставляется через договор управления
        ContractTsgGkh, //Предоставляется через договор с ТСЖ и ЖСК
        DirectContract //Предоставляется через прямые договоры с собственниками
    }
}