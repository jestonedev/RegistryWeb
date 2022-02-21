using System;

namespace RegistryDb.Models.Entities
{
    public partial class OwnerReason : IEquatable<OwnerReason>
    {
        public int IdReason { get; set; }
        public int IdOwner { get; set; }
        public int NumeratorShare { get; set; } = 1;
        public int DenominatorShare { get; set; } = 1;
        public int IdReasonType { get; set; }
        public string ReasonNumber { get; set; }
        public DateTime ReasonDate { get; set; }
        public byte Deleted { get; set; }

        public virtual Owner IdOwnerNavigation { get; set; }
        public virtual OwnerReasonType IdReasonTypeNavigation { get; set; }

        public bool Equals(OwnerReason or)
        {
            if (or == null)
                return false;
            if (ReferenceEquals(this, or))
                return true;
            return IdReason == or.IdReason && IdOwner == or.IdOwner &&
                NumeratorShare == or.NumeratorShare && DenominatorShare == or.DenominatorShare &&
                IdReasonType == or.IdReasonType && ReasonNumber == or.ReasonNumber &&
                ReasonDate == or.ReasonDate && Deleted == or.Deleted;
        }
    }
}
