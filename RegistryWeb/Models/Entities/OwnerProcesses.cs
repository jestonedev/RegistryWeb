using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerProcesses
    {
        public OwnerProcesses()
        {
            OwnerBuildingsAssoc = new List<OwnerBuildingsAssoc>();
            OwnerOrginfos = new List<OwnerOrginfos>();
            OwnerPersons = new List<OwnerPersons>();
            OwnerReasons = new List<OwnerReasons>();
            OwnerPremisesAssoc = new List<OwnerPremisesAssoc>();
            OwnerSubPremisesAssoc = new List<OwnerSubPremisesAssoc>();
        }

        public int IdProcess { get; set; }
        public int IdOwnerType { get; set; }
        public DateTime? AnnulDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Comment { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerType IdOwnerTypeNavigation { get; set; }
        public virtual IList<OwnerBuildingsAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual IList<OwnerOrginfos> OwnerOrginfos { get; set; }
        public virtual IList<OwnerPersons> OwnerPersons { get; set; }
        public virtual IList<OwnerReasons> OwnerReasons { get; set; }
        public virtual IList<OwnerPremisesAssoc> OwnerPremisesAssoc { get; set; }
        public virtual IList<OwnerSubPremisesAssoc> OwnerSubPremisesAssoc { get; set; }
    }
}
