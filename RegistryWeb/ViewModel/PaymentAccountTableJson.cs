using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class PaymentAccountTableJson
    {
        public bool HasAccount { get; set; }
        public bool HasTenant { get; set; }
        public bool HasTotalArea { get; set; }
        public bool HasLivingArea { get; set; }
        public bool HasPrescribed { get; set; }
        public bool HasBalanceInput { get; set; }
        public bool HasBalanceTenancy { get; set; }
        public bool HasBalanceInputPenalties { get; set; }
        public bool HasBalanceDgi { get; set; }
        public bool HasBalancePadun { get; set; }
        public bool HasBalancePkk { get; set; }
        public bool HasChargingTotal { get; set; }
        public bool HasChargingTenancy { get; set; }
        public bool HasChargingPenalties { get; set; }
        public bool HasChargingDgi { get; set; }
        public bool HasChargingPadun { get; set; }
        public bool HasChargingPkk { get; set; }
        public bool HasTransferBalance { get; set; }
        public bool HasRecalcTenancy { get; set; }
        public bool HasRecalcPenalties { get; set; }
        public bool HasRecalcDgi { get; set; }
        public bool HasRecalcPadun { get; set; }
        public bool HasRecalcPkk { get; set; }
        public bool HasPaymentTenancy { get; set; }
        public bool HasPaymentPenalties { get; set; }
        public bool HasPaymentDgi { get; set; }
        public bool HasPaymentPadun { get; set; }
        public bool HasPaymentPkk { get; set; }
        public bool HasBalanceOutputTotal { get; set; }
        public bool HasBalanceOutputTenancy { get; set; }
        public bool HasBalanceOutputPenalties { get; set; }
        public bool HasBalanceOutputDgi { get; set; }
        public bool HasBalanceOutputPadun { get; set; }
        public bool HasBalanceOutputPkk { get; set; }
    }
}
