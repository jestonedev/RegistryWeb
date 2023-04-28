using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryDb.Models.Entities.Payments
{
    public class PaymentAccountComment
    {
        public int Id { get; set; }
        public int IdAccount { get; set; }
        public string Comment { get; set; }
    }
}
