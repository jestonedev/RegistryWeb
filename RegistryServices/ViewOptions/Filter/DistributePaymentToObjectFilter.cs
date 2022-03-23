using RegistryServices.Enums;
using System;
using System.Collections.Generic;

namespace RegistryWeb.ViewOptions.Filter
{
    public class DistributePaymentToObjectFilter : TenancyProcessesFilter
    {
        public KumiPaymentDistributeToEnum DistributeTo { get; set; }

        public string ClaimCourtOrderNum { get; set; }
        public DateTime? ClaimAtDate { get; set; }
        public int? ClaimIdStateType { get; set; }

        public string AccountGisZkh { get; set; }
        public string Account { get; set; }
        public int? IdAccountState { get; set; }

        public new bool IsEmpty()
        {
            return IsTenancyEmpty() && IsClaimEmpty() && IsAccountEmpty();
        }

        public bool IsTenancyEmpty()
        {
            return base.IsEmpty();
        }

        public bool IsAccountEmpty()
        {
            return AccountGisZkh == null && Account == null && IdAccountState == null;
        }

        public bool IsClaimEmpty()
        {
            return (ClaimCourtOrderNum == null && ClaimAtDate == null && ClaimIdStateType == null) || DistributeTo != KumiPaymentDistributeToEnum.ToClaim;
        }
    }
}
