using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class Restriction
    {
        public Restriction()
        {
            RestrictionBuildingsAssoc = new List<RestrictionBuildingAssoc>();
            RestrictionPremisesAssoc = new List<RestrictionPremiseAssoc>();
        }

        public int IdRestriction { get; set; }
        public int IdRestrictionType { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public DateTime? DateStateReg { get; set; }
        public string FileOriginName { get; set; }
        public string FileDisplayName { get; set; }
        public string FileMimeType { get; set; }
        public byte Deleted { get; set; }

        public virtual RestrictionType RestrictionTypeNavigation { get; set; }
        public virtual IList<RestrictionBuildingAssoc> RestrictionBuildingsAssoc { get; set; }
        public virtual IList<RestrictionPremiseAssoc> RestrictionPremisesAssoc { get; set; }
    }
}
