using System;

namespace RegistryDb.Models.Entities
{
    public class KumiPaymentGroupFile
    {
        public int IdFile { get; set; }
        public int IdGroup { get; set; }
        public string FileName { get; set; }
        public string FileVersion { get; set; }
        public virtual KumiPaymentGroup PaymentGroup { get; set; }
    }
}