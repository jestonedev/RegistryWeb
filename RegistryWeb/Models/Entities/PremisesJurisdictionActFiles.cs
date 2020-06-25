﻿using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesJurisdictionActFiles
    {
        public int IdJurisdiction { get; set; }
        public int IdPremises { get; set; }
        public int? IdActFile { get; set; }
        public int IdActFileTypeDocument { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string FileOriginName { get; set; }
        public string FileDisplayName { get; set; }
        public string FileMimeType { get; set; }
        public string Name { get; set; }
        public byte Deleted { get; set; }

        public virtual Premise PremiseNavigation { get; set; }
        public virtual ActTypeDocument IdActFileTypeDocumentNavigation { get; set; }
    }
}
