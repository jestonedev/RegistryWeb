﻿using RegistryDb.Models.Entities.Owners;
using RegistryWeb.Enums;

namespace RegistryServices.ViewModel.Owners
{
    public class OwnerFileVM
    {
        public OwnerFile OwnerFile { get; set; }
        public int I { get; set; }
        public ActionTypeEnum Action { get; set; }
    }
}
