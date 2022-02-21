using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public partial class ResettleStage
    {
        public ResettleStage()
        {
            ResettleInfos = new List<ResettleInfo>();
        }

        public int IdResettleStage { get; set; }
        public string StageName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<ResettleInfo> ResettleInfos { get; set; }
    }
}
