using System.Xml.Serialization;

namespace RegistryWeb.Models.Api
{
    public enum CommunalServiceMethodEnum
    {
        [XmlEnum(Name = "1")]
        ManagementContract = 1, //Предоставляется через договор управления

        [XmlEnum(Name = "2")]
        ContractTsgGkh, //Предоставляется через договор с ТСЖ и ЖСК

        [XmlEnum(Name = "3")]
        DirectContract //Предоставляется через прямые договоры с собственниками
    }
}