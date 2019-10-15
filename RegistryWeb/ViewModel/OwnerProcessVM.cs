using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessVM
    {
        public OwnerProcess OwnerProcess { get; set; }
        public IList<LogOwnerProcess> Logs { get; set; }
    }
}