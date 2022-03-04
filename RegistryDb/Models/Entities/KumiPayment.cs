using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public class KumiPayment
    {
        public KumiPayment()
        {
            PaymentCharges = new List<KumiPaymentCharge>();
        }

        public int IdPayment { get; set; }
        public int IdGroup { get; set; }
        public int IdSource { get; set; }
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
        public byte IsPosted { get; set; }
        public byte Deleted { get; set; }
        public virtual IList<KumiPaymentCharge> PaymentCharges { get; set; }
        public virtual IList<KumiPaymentClaim> PaymentClaims { get; set; }
        public virtual IList<KumiPaymentUf> PaymentUfs { get; set; }
        public virtual KumiPaymentGroup PaymentGroup { get; set; }
        public virtual KumiPaymentInfoSource PaymentInfoSource { get; set; }
        public virtual KumiPaymentKind PaymentKind { get; set; }
        public virtual KumiOperationType OperationType { get; set; }
        public virtual KumiKbkType KbkType { get; set; }
        public virtual KumiPaymentReason PaymentReason { get; set; }
        public virtual KumiPayerStatus PayerStatus { get; set; }
    }
}
