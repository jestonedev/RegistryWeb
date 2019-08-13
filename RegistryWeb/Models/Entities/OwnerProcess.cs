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
            OwnerOrginfos = new List<OwnerOrginfo>();
            OwnerPersons = new List<OwnerPerson>();
            OwnerPremisesAssoc = new List<OwnerPremiseAssoc>();
            OwnerSubPremisesAssoc = new List<OwnerSubPremiseAssoc>();
        }

        public int IdProcess { get; set; }
        [Range(1, 3)]
        public int IdOwnerType { get; set; }
        public DateTime? AnnulDate { get; set; }
        public string AnnulComment { get; set; }
        public string Comment { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerType IdOwnerTypeNavigation { get; set; }
        public virtual IList<OwnerBuildingAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual IList<OwnerOrginfo> OwnerOrginfos { get; set; }
        public virtual IList<OwnerPerson> OwnerPersons { get; set; }
        public virtual IList<OwnerPremiseAssoc> OwnerPremisesAssoc { get; set; }
        public virtual IList<OwnerSubPremiseAssoc> OwnerSubPremisesAssoc { get; set; }
    }
}
