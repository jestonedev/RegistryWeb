using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class LitigationType
    {
        public LitigationType()
        {
            Litigations = new List<Litigation>();
        }

        public int IdLitigationType { get; set; }
        public string LitigationTypeName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<Litigation> Litigations { get; set; }
    }
}
