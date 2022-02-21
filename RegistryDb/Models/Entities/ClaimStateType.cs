using System.Collections.Generic;

namespace RegistryDb.Models.Entities
{
    public class ClaimStateType
    {
        public ClaimStateType()
        {
            ClaimStates = new List<ClaimState>();
        }

        public int IdStateType { get; set; }
        public string StateType { get; set; }
        public bool IsStartStateType { get; set; }
        public byte Deleted { get; set; }
        public virtual ICollection<ClaimState> ClaimStates { get; set; }
    }
}
