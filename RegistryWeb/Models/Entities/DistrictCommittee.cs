using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class DistrictCommittee
    {
        public int IdCommittee { get; set; }
        public string NameNominative { get; set; }
        public string NameGenetive { get; set; }
        public string NamePrepositional { get; set; }
        public string HeadSnpGenetive { get; set; }
        public string HeadPostGenetive { get; set; }
        public string HeadSnp { get; set; }
        public string HeadPost { get; set; }
    }
}
