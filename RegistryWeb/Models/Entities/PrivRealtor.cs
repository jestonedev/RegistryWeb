using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PrivRealtor
    {
        public PrivRealtor()
        {
        }

        public int IdRealtor { get; set; }
        public string Name { get; set; }
        public string Passport { get; set; }
        public DateTime? DateBirth { get; set; }
        public string PlaceOfRegistration { get; set; }
        public byte Deleted { get; set; }
    }
}
