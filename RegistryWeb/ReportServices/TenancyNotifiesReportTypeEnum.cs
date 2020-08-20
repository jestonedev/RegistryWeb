using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ReportServices
{
    public enum TenancyNotifiesReportTypeEnum
    {
        ExportAsIs = 1,
        PrintNotifiesPrimary,
        PrintNotifiesSecondary,
        PrintNotifiesProlongContract,
        PrintNotifiesEvictionFromEmergencyFund
    }
}
