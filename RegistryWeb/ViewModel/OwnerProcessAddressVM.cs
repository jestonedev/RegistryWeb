﻿using System;
using RegistryWeb.Models;

namespace RegistryWeb.ViewModel
{
    public class OwnerProcessAddressVM
    {
        public int IdProcess { get; set; }
        public int IdAssoc { get; set; }
        public int Id { get; set; }
        public int I { get; set; }
        public AddressTypes AddressType { get; set; }        
        public ActionTypeEnum Action { get; set; }
        public string Text { get; set; }
        public string Href { get; set; }
    }
}