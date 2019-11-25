using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerProcess : IEquatable<OwnerProcess>
    {
        public OwnerProcess()
        {
            OwnerBuildingsAssoc = new List<OwnerBuildingAssoc>();            
            OwnerPremisesAssoc = new List<OwnerPremiseAssoc>();
            OwnerSubPremisesAssoc = new List<OwnerSubPremiseAssoc>();
            Owners = new List<Owner>();
        }

        public int IdProcess { get; set; }
        public DateTime? AnnulDate { get; set; }
        public string AnnulComment { get; set; }
        public string Comment { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerActiveProcess OwnerActiveProcessNavigation { get; set; }
        public virtual IList<OwnerBuildingAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual IList<OwnerPremiseAssoc> OwnerPremisesAssoc { get; set; }
        public virtual IList<OwnerSubPremiseAssoc> OwnerSubPremisesAssoc { get; set; }
        public virtual IList<Owner> Owners { get; set; }

        public bool Equals(OwnerProcess op)
        {
            if (op == null)
                return false;
            if (ReferenceEquals(this, op))
                return true;
            return IdProcess == op.IdProcess && AnnulDate == op.AnnulDate &&
                AnnulComment == op.AnnulComment && Comment == op.Comment && Deleted == op.Deleted &&
                OwnerBuildingsAssoc.SequenceEqual(op.OwnerBuildingsAssoc) &&
                OwnerPremisesAssoc.SequenceEqual(op.OwnerPremisesAssoc) &&
                OwnerSubPremisesAssoc.SequenceEqual(op.OwnerSubPremisesAssoc) &&
                Owners.SequenceEqual(op.Owners);
        }
    }
}
