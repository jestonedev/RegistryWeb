﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class PersonalSetting
    {
        public int IdUser { get; set; }
        public string PaymentAccauntTableJson { get; set; }

        public virtual AclUser AclUser { get; set; }

    }
}
