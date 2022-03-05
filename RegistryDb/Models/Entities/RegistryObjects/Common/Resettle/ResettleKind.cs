using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Common.Resettle
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
