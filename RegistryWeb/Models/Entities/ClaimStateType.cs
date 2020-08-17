using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models.Entities
{
    public class ClaimStateType
    {
        public int IdStateType { get; set; }
        public string StateType { get; set; }
        public byte IsStartStateType { get; set; }
        public byte Deleted { get; set; }
        public virtual IList<ClaimState> ClaimStates { get; set; }
    }
}
