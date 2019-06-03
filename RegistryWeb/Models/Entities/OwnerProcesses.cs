using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerProcesses
    {
        public OwnerProcesses()
        {
            OwnerBuildingsAssoc = new HashSet<OwnerBuildingsAssoc>();
            OwnerOrginfos = new HashSet<OwnerOrginfos>();
            OwnerPersons = new HashSet<OwnerPersons>();
            OwnerReasons = new HashSet<OwnerReasons>();
            OwnerPremisesAssoc = new HashSet<OwnerPremisesAssoc>();
            OwnerSubPremisesAssoc = new HashSet<OwnerSubPremisesAssoc>();
        }

        public int IdProcess { get; set; }
        public int IdOwnerType { get; set; }
        public DateTime? AnnulDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Comment { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerType IdOwnerTypeNavigation { get; set; }
        public virtual ICollection<OwnerBuildingsAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual ICollection<OwnerOrginfos> OwnerOrginfos { get; set; }
        public virtual ICollection<OwnerPersons> OwnerPersons { get; set; }
        public virtual ICollection<OwnerReasons> OwnerReasons { get; set; }
        public virtual ICollection<OwnerPremisesAssoc> OwnerPremisesAssoc { get; set; }
        public virtual ICollection<OwnerSubPremisesAssoc> OwnerSubPremisesAssoc { get; set; }
    }
}
