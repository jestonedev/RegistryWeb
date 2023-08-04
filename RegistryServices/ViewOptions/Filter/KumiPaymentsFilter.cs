using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.ViewOptions.Filter
{
    public class KumiPaymentsFilter: FilterOptions
    {
        public string CommonFilter { get; set; }
        public int? IdParentPayment { get; set; }
        // Modal
        public List<int> IdsSource { get; set; }
        public string NumDocument { get; set; }
        public DateTime? DateDocument { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateExecute { get; set; }
        public DateTime? DateEnrollUfk { get; set; }
        public string Uin { get; set; }
        public decimal? Sum { get; set; }
        public string Purpose { get; set; }
        public string Kbk { get; set; }
        public string Okato { get; set; }

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

        public bool? IsPosted { get; set; }
        public DateTime? LoadDate { get; set; }

        // Filter by period refs
        public int? IdCharge { get; set; }
        public int? IdClaim { get; set; }

        // Not actual
        public int? IdAccount { get; set; }

        public KumiPaymentsFilter()
        {
            Kbk = "90111109044041000120";
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(CommonFilter) && IdParentPayment == null && IsModalEmpty()
                && IsRefEmpty();
        }

        public bool IsRefEmpty()
        {
            return IdAccount == null && IdCharge == null && IdClaim == null;
        }

        public bool IsModalEmpty()
        {
            return (IdsSource == null || IdsSource.Count == 0) 
                 && NumDocument == null && DateDocument == null && DateIn == null && DateExecute == null && DateEnrollUfk == null &&
                 Uin == null && Sum == null && Purpose == null && Kbk == null && Okato == null && PayerInn == null &&
                 PayerKpp == null && PayerName == null && PayerAccount == null && PayerBankName == null &&
                 PayerBankBik == null && PayerBankAccount == null && RecipientInn == null && RecipientKpp == null &&
                 RecipientName == null && RecipientAccount == null && RecipientBankAccount == null &&
                 RecipientBankBik == null && RecipientBankName == null && IsPosted == null && LoadDate == null;
        }
    }
}
