using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.Models.KumiAccounts
{
    public class SplitAccountModel
    {
        public string Account { get; set; }
        public string Owner { get; set; }
        public decimal Fraction { get; set; }
    }
}
