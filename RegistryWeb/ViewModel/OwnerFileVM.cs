﻿using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.Enums;

namespace RegistryWeb.ViewModel
{
    public class OwnerFileVM
    {
        public OwnerFile OwnerFile { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}
