using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.KumiAccounts
{
    public class KumiPaymentSettingSet
    {
        public int IdSettingSet { get; set; }
        public string CodeUbp { get; set; }
        public string NameUbp { get; set; }
        public string AccountUbp { get; set; }
        public string NameGrs { get; set; }
        public string GlavaGrs { get; set; }
        public string OkpoFo { get; set; }
        public string NameFo { get; set; }
        public string AccountFo { get; set; }
        public string CodeTofk { get; set; }
        public string NameTofk { get; set; }
        public string NameBudget { get; set; }
        public string BudgetLevel { get; set; }

    }
}