using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts
{
    public class SberbankFileRow
    {
        [Key]
        public int Id { get; set; }
        public string Account { get; set; }
        public string Tenant { get; set; }
        public string Address { get; set; }
        public string Kbk { get; set; }
        public string Okato { get; set; }
        public string Sum { get; set; }
    }
}
