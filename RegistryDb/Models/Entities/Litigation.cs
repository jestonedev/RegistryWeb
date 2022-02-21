using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class Litigation
    {
        public Litigation()
        {
            LitigationPremisesAssoc = new List<LitigationPremiseAssoc>();
        }

        public int IdLitigation { get; set; }
        public int IdLitigationType { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string FileOriginName { get; set; }
        public string FileDisplayName { get; set; }
        public string FileMimeType { get; set; }
        public byte Deleted { get; set; }
        public virtual IList<LitigationPremiseAssoc> LitigationPremisesAssoc { get; set; }
        public virtual LitigationType LitigationTypeNavigation { get; set; }
    }
}
