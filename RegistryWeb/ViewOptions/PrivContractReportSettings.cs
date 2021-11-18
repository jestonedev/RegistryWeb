using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions
{
    public class PrivContractReportSettings
    {
        public int IdContract { get; set; }
        public int IdOwner { get; set; }
        public int? IdOwnerSigner { get; set; }
        public PrivContractTypeEnum ContractType { get; set; }
        public PrivContractKindEnum ContractKind { get; set; }
    }
}
