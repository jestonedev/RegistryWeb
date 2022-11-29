using System;

namespace RegistryDb.Models.Entities.Payments
{
    public class LogInvoiceGenerator
    {
        public int Id { get; set; }
        public int IdAccount { get; set; }
        public int AccountType { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime OnDate { get; set; }
        public string Emails { get; set; }
        public int ResultCode { get; set; }
    }
}
