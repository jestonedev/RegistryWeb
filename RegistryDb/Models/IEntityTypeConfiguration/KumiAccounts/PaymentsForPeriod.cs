using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class PaymentsForPeriod
    {
        [Key]
        public int Id { get; set; }
        public string num_d { get; set; }
        public string date_d_str { get; set; }
        public string payer_name { get; set; }
        public decimal sum { get; set; }
        public string purpose { get; set; }
        public string note { get; set; }
        public string account_info { get; set; }
        public int group_index { get; set; }
    }
}


