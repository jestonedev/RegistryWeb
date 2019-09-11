﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class LogOwnerProcessValue
    {
        public int Id { get; set; }
        public int IdLog { get; set; }
        public string Talble { get; set; }
        public int IdKey { get; set; }
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public virtual LogOwnerProcess IdLogNavigation { get; set; }
    }
}
