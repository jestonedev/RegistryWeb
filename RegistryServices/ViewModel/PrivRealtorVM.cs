﻿using RegistryDb.Models.Entities;
using RegistryWeb.ViewOptions;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class PrivRealtorVM<T>
    {
        public List<PrivRealtor> PrivRealtors { get; set; }
        public PrivRealtor PrivRealtor { get; set; }
        public PageOptions PageOptions { get; set; }
    }
}
