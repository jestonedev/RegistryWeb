using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryWeb.Models.SqlViews;

namespace RegistryWeb.Models.Entities
{
    public class AccountBase
    {
        public AccountBase()
        {
        }

        public int IdAccount { get; set; }
        public string Account { get; set; }
        public string AccountGisZkh { get; set; }
    }
}
