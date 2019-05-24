using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class SubPremises
    {
        public SubPremises()
        {
            FundsSubPremisesAssoc = new HashSet<FundsSubPremisesAssoc>();
            OwnerSubPremisesAssoc = new HashSet<OwnerSubPremisesAssoc>();
        }

        public int IdSubPremises { get; set; }
        public int IdPremises { get; set; }
        public int IdState { get; set; }
        public string SubPremisesNum { get; set; }
        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
        public string Description { get; set; }
        public DateTime? StateDate { get; set; }
        public string CadastralNum { get; set; }
        public decimal CadastralCost { get; set; }
        public decimal BalanceCost { get; set; }
        public string Account { get; set; }
        public byte Deleted { get; set; }

        public virtual Premises IdPremisesNavigation { get; set; }
        public virtual ObjectStates IdStateNavigation { get; set; }
        public virtual ICollection<FundsSubPremisesAssoc> FundsSubPremisesAssoc { get; set; }
        public virtual ICollection<OwnerSubPremisesAssoc> OwnerSubPremisesAssoc { get; set; }
    }
}
