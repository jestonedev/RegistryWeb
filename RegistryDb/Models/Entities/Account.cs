using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryDb.Models.SqlViews;

namespace RegistryDb.Models.Entities
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
