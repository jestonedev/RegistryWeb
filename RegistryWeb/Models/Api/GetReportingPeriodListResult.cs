using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RegistryWeb.Models.Api
{
    [CollectionDataContract(ItemName = "item"), KnownType(typeof(List<ReportingPeriod>))]
    public class ReportingPeriodList : List<ReportingPeriod> {}
}
