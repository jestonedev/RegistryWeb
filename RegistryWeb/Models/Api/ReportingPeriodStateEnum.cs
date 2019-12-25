using System.Runtime.Serialization;

namespace RegistryWeb.Models.Api
{
    [DataContract]
    public enum ReportingPeriodStateEnum
    {
        [EnumMember(Value = "1")]
        Current = 1,
        [EnumMember(Value = "2")]
        Archive
    }
}
