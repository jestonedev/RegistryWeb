﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class ResettleInfo
    {
        public ResettleInfo()
        {
            ResettlePremisesAssoc = new List<ResettlePremiseAssoc>();
            ResettleDocuments = new List<ResettleDocument>();
            ResettleInfoSubPremisesFrom = new List<ResettleInfoSubPremiseFrom>();
            ResettleInfoTo = new List<ResettleInfoTo>();
            ResettleInfoToFact = new List<ResettleInfoToFact>();
        }

        public int IdResettleInfo { get; set; }
        public DateTime? ResettleDate { get; set; }
        public int? IdResettleKind { get; set; }
        public int? IdResettleKindFact { get; set; }
        [Required]
        public decimal FinanceSource1 { get; set; }
        [Required]
        public decimal FinanceSource2 { get; set; }
        [Required]
        public decimal FinanceSource3 { get; set; }
        [Required]
        public decimal FinanceSource4 { get; set; }
        public byte Deleted { get; set; }

        public virtual ResettleKind ResettleKindNavigation { get; set; }
        public virtual ResettleKind ResettleKindFactNavigation { get; set; }
        public virtual IList<ResettlePremiseAssoc> ResettlePremisesAssoc { get; set; }
        public virtual IList<ResettleDocument> ResettleDocuments { get; set; }
        public virtual IList<ResettleInfoSubPremiseFrom> ResettleInfoSubPremisesFrom  { get; set; }
        public virtual IList<ResettleInfoTo> ResettleInfoTo { get; set; }   // Планируемый адрес переселения
        public virtual IList<ResettleInfoToFact> ResettleInfoToFact { get; set; }   // Фактический адрес переселения
    }
}
