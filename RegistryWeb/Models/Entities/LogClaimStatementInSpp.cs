﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryWeb.Models.Entities
{
    public class LogClaimStatementInSpp
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string ExecutorLogin { get; set; }
        public int IdClaim { get; set; }
    }
}
