using System;
using RegistryDb.Models.Entities.KumiAccounts;

namespace RegistryPaymentsLoader.Models
{
    [Serializable]
    public class KumiPaymentExtract
    {
        public string Guid { get; set; }
        public string CodeDoc { get; set; }
        public string NumDoc { get; set; }
        public DateTime DateDoc { get; set; }
        public string CodeDocAdp { get; set; }
        public decimal SumIn { get; set; }
        public decimal SumOut { get; set; }
        public decimal SumZach { get; set; }
        public string Kbk { get; set; }
        public string KbkType { get; set; }
        public string TargetCode { get; set; }
        public string Okato { get; set; }
        public string InnAdb { get; set; }
        public string KppAdb { get; set; }
        public DateTime? DateEnrollUfk { get; set; }

        public bool IsMemorialOrder() {
            return CodeDoc == "UF";
        }

        public KumiMemorialOrder ToMemorialOrder()
        {
            return new KumiMemorialOrder
            {
                NumDocument = NumDoc,
                DateDocument = DateDoc,
                Guid = Guid,
                SumIn = SumIn,
                SumZach = SumZach,
                Kbk = Kbk,
                KbkType = new KumiKbkType {
                    Code = KbkType
                },
                TargetCode = TargetCode,
                Okato = Okato,
                InnAdb = InnAdb,
                KppAdb = KppAdb,
                DateEnrollUfk = DateEnrollUfk
            };
        }
    }
}