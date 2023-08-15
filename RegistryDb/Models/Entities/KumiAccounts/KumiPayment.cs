using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json.Serialization;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    [Serializable]
    public class KumiPayment
    {
        public KumiPayment()
        {
            PaymentCharges = new List<KumiPaymentCharge>();
            PaymentClaims = new List<KumiPaymentClaim>();
            PaymentUfs = new List<KumiPaymentUf>();
            PaymentCorrections = new List<KumiPaymentCorrection>();
            MemorialOrderPaymentAssocs = new List<KumiMemorialOrderPaymentAssoc>();
            ChildPayments = new List<KumiPayment>();
        }

        public int IdPayment { get; set; }
        public int? IdParentPayment { get; set; }
        public int? IdGroup { get; set; }
        public int IdSource { get; set; }
        public int? IdPaymentDocCode { get; set; }
        public string Guid { get; set; }
        public string NumDocument { get; set; }
        public DateTime? DateDocument { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateExecute { get; set; }
        public DateTime? DatePay { get; set; }
        public int? IdPaymentKind { get; set; }
        public int? OrderPay { get; set; }
        public int? IdOperationType { get; set; }
        public decimal Sum { get; set; }
        public string Uin { get; set; }
        public int? IdPurpose { get; set; }
        public string Purpose { get; set; }
        public string Kbk { get; set; }
        public int? IdKbkType { get; set; }
        public string TargetCode { get; set; }
        public string Okato { get; set; }
        public int? IdPaymentReason { get; set; }
        public string NumDocumentIndicator { get; set; }
        public DateTime? DateDocumentIndicator { get; set; }
        public int? IdPayerStatus { get; set; }
        public string PayerInn { get; set; }
        public string PayerKpp { get; set; }
        public string PayerName { get; set; }
        public string PayerAccount { get; set; }
        public string PayerBankBik { get; set; }
        public string PayerBankName { get; set; }
        public string PayerBankAccount { get; set; }
        public string RecipientInn { get; set; }
        public string RecipientKpp { get; set; }
        public string RecipientName { get; set; }
        public string RecipientAccount { get; set; }
        public string RecipientBankBik { get; set; }
        public string RecipientBankName { get; set; }
        public string RecipientBankAccount { get; set; }
        public string Description { get; set; } 
        public DateTime? DateEnrollUfk { get; set; }
        public byte IsPosted { get; set; }
        public byte IsConsolidated { get; set; }
        public byte Deleted { get; set; }
        public virtual IList<KumiPaymentCharge> PaymentCharges { get; set; }
        public virtual IList<KumiPaymentClaim> PaymentClaims { get; set; }
        public virtual IList<KumiPaymentUf> PaymentUfs { get; set; }
        public virtual IList<KumiPaymentCorrection> PaymentCorrections { get; set; }
        public virtual IList<KumiMemorialOrderPaymentAssoc> MemorialOrderPaymentAssocs { get; set; }
        public virtual KumiPaymentGroup PaymentGroup { get; set; }
        public virtual KumiPaymentInfoSource PaymentInfoSource { get; set; }
        public virtual KumiPaymentDocCode PaymentDocCode { get; set; }
        public virtual KumiPaymentKind PaymentKind { get; set; }
        public virtual KumiOperationType OperationType { get; set; }
        public virtual KumiKbkType KbkType { get; set; }
        public virtual KumiPaymentReason PaymentReason { get; set; }
        public virtual KumiPayerStatus PayerStatus { get; set; }
        public virtual KumiPayment ParentPayment { get; set; }
        public virtual IList<KumiPayment> ChildPayments { get; set; }

        public KumiPayment Copy(bool childPayment)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;

                var copyPayment = (KumiPayment)formatter.Deserialize(ms);
                // Если платеж копируется как дочерний, то обнуляем идентификаторы и убираем уведомления об уточнении
                if (childPayment)
                {
                    copyPayment.IdParentPayment = copyPayment.IdPayment;
                    copyPayment.IdPayment = 0;
                    if (copyPayment.MemorialOrderPaymentAssocs != null)
                    {
                        foreach (var order in copyPayment.MemorialOrderPaymentAssocs)
                        {
                            order.IdPayment = 0;
                            order.IdAssoc = 0;
                        }
                    }
                    if (copyPayment.PaymentUfs != null)
                    {
                        copyPayment.PaymentUfs = null;
                    }
                    if (copyPayment.PaymentCorrections != null)
                    {
                        foreach (var correction in copyPayment.PaymentCorrections)
                        {
                            correction.IdPayment = 0;
                            correction.IdCorrection = 0;
                        }
                    }
                }
                return copyPayment;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is KumiPayment)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Guid == ((KumiPayment)obj).Guid;
        }

        public override int GetHashCode()
        {
            return Guid?.GetHashCode() ?? 0;
        }
    }
}
