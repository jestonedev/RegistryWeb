using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class Premises
    {
        public Premises()
        {
            FundsPremisesAssoc = new HashSet<FundsPremisesAssoc>();
            OwnerPremisesAssoc = new HashSet<OwnerPremisesAssoc>();
            OwnershipPremisesAssoc = new HashSet<OwnershipPremisesAssoc>();
            SubPremises = new HashSet<SubPremises>();
        }

        public int IdPremises { get; set; }
        public int IdBuilding { get; set; }
        public int IdState { get; set; }
        public int IdPremisesKind { get; set; }
        public int IdPremisesType { get; set; }
        public int IdPremisesComment { get; set; }
        public int IdPremisesDoorKeys { get; set; }
        public string PremisesNum { get; set; }
        public short Floor { get; set; }
        public short NumRooms { get; set; }
        public short NumBeds { get; set; }
        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
        public double Height { get; set; }
        public string CadastralNum { get; set; }
        public decimal CadastralCost { get; set; }
        public decimal BalanceCost { get; set; }
        public string Description { get; set; }
        public DateTime RegDate { get; set; }
        public byte IsMemorial { get; set; }
        public string Account { get; set; }
        public DateTime? StateDate { get; set; }
        public byte Deleted { get; set; }

        public virtual Buildings IdBuildingNavigation { get; set; }
        public virtual PremisesComments IdPremisesCommentNavigation { get; set; }
        public virtual PremisesKinds IdPremisesKindNavigation { get; set; }
        public virtual PremisesTypes IdPremisesTypeNavigation { get; set; }
        public virtual ObjectStates IdStateNavigation { get; set; }
        public virtual ICollection<FundsPremisesAssoc> FundsPremisesAssoc { get; set; }
        public virtual ICollection<OwnerPremisesAssoc> OwnerPremisesAssoc { get; set; }
        public virtual ICollection<OwnershipPremisesAssoc> OwnershipPremisesAssoc { get; set; }
        public virtual ICollection<SubPremises> SubPremises { get; set; }
    }
}
