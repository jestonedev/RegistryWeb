using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesJurisdictionActFiles
    {
        /*public PremisesJurisdictionActFiles()
        {
            Premises = new List<Premise>();
        }*/

        public int IdJurisdiction { get; set; }
        public int IdPremises { get; set; }
        public int? IdActFile { get; set; }
        public int IdActFileTypeDocument { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public byte Deleted { get; set; }

        //public virtual IList<Premise> Premises { get; set; }
        public virtual Premise PremiseNavigation { get; set; }
        public virtual ActTypeDocument IdActFileTypeDocumentNavigation { get; set; }
    }
}
