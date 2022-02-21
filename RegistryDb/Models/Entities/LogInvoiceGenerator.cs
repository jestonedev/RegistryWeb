using System;

namespace RegistryDb.Models
{
    public class LogInvoiceGenerator
    {
        public int Id { get; set; }
        public int IdAccount { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime OnDate { get; set; }
        public string Emails { get; set; }
        public int ResultCode { get; set; }
    }
}
