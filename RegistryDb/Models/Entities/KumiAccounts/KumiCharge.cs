using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiCharge
    {
        public KumiCharge()
        {
            PaymentCharges = new List<KumiPaymentCharge>();
        }

        public int IdCharge { get; set; }
        public int IdAccount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        // Найм
        public decimal InputTenancy { get; set; }
        public decimal InputPenalty { get; set; }
        public decimal ChargeTenancy { get; set; }
        public decimal ChargePenalty { get; set; }
        public decimal PaymentTenancy { get; set; }
        public decimal PaymentPenalty { get; set; }
        public decimal RecalcTenancy { get; set; }
        public decimal RecalcPenalty { get; set; }
        public decimal CorrectionTenancy { get; set; }
        public decimal CorrectionPenalty { get; set; }
        public decimal OutputTenancy { get; set; }
        public decimal OutputPenalty { get; set; }

        // ДГИ
        public decimal InputDgi { get; set; }
        public decimal ChargeDgi { get; set; }
        public decimal PaymentDgi { get; set; }
        public decimal RecalcDgi { get; set; }
        public decimal CorrectionDgi { get; set; }
        public decimal OutputDgi { get; set; }

        // ПКК
        public decimal InputPkk { get; set; }
        public decimal ChargePkk { get; set; }
        public decimal PaymentPkk { get; set; }
        public decimal RecalcPkk { get; set; }
        public decimal CorrectionPkk { get; set; }
        public decimal OutputPkk { get; set; }

        // Падун
        public decimal InputPadun { get; set; }
        public decimal ChargePadun { get; set; }
        public decimal PaymentPadun { get; set; }
        public decimal RecalcPadun { get; set; }
        public decimal CorrectionPadun { get; set; }
        public decimal OutputPadun { get; set; }

        public byte Deleted { get; set; }
        public byte Hidden { get; set; }
        public byte IsBksCharge { get; set; }
        public virtual KumiAccount Account { get; set; }
        public virtual IList<KumiPaymentCharge> PaymentCharges { get; set; }
        public virtual IList<KumiPaymentCharge> DisplayPaymentCharges { get; set; }
        public virtual IList<KumiPaymentClaim> DisplayPaymentClaims { get; set; }
        public virtual IList<KumiPaymentUntied> UntiedPaymentsInfo { get; set; }

        public static bool operator==(KumiCharge first, KumiCharge second)
        {
            if ((object)first == null && (object)second == null)
                return true;
            if ((object)first == null || (object)second == null)
                return false;
            foreach (var propertyInfo in first.GetType().GetProperties())
            {
                if (propertyInfo.Name == "Account" || propertyInfo.Name == "PaymentCharges") continue;
                var firstValue = propertyInfo.GetValue(first, null);
                var secondValue = propertyInfo.GetValue(second, null);
                if (firstValue == null && secondValue == null)
                    continue;
                if (firstValue == null || secondValue == null)
                    return false;
                if (!firstValue.Equals(secondValue))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool operator !=(KumiCharge first, KumiCharge second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            return this == obj as KumiCharge;
        }

        public bool Equals(KumiCharge other)
        {
            return Equals((object)other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
