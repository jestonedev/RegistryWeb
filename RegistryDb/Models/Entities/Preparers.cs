using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class Preparer
    {
        public int IdPreparer { get; set; }
        public string PreparerName { get; set; }
        public string Position { get; set; }
        public string ShortPosition { get; set; }
    }
}
