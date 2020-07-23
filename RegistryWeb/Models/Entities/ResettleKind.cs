using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class ResettleKind
    {
        public ResettleKind()
        {
            ResettleInfos = new List<ResettleInfo>();
            ResettleInfosFact = new List<ResettleInfo>();
        }

        public int IdResettleKind { get; set; }
        public string ResettleKindName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<ResettleInfo> ResettleInfos { get; set; }
        public virtual IList<ResettleInfo> ResettleInfosFact { get; set; }
    }
}
