﻿using RegistryWeb.Models;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.ViewModel
{
    public class OwnerFileVM
    {
        public OwnerFile OwnerFile { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}