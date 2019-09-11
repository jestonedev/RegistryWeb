using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerProcess
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

        public virtual IList<OwnerBuildingAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual IList<OwnerPremiseAssoc> OwnerPremisesAssoc { get; set; }
        public virtual IList<OwnerSubPremiseAssoc> OwnerSubPremisesAssoc { get; set; }
        public virtual IList<Owner> Owners { get; set; }
    }
}
