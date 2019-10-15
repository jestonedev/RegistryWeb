using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class Premise : IAddress
    {
        public Premise()
        {
            FundsPremisesAssoc = new List<FundPremiseAssoc>();
            OwnerPremisesAssoc = new List<OwnerPremiseAssoc>();
            OwnershipPremisesAssoc = new List<OwnershipPremiseAssoc>();
            SubPremises = new List<SubPremise>();
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

        public virtual Building IdBuildingNavigation { get; set; }
        public virtual PremisesComment IdPremisesCommentNavigation { get; set; }
        public virtual PremisesKind IdPremisesKindNavigation { get; set; }
        public virtual PremisesType IdPremisesTypeNavigation { get; set; }
        public virtual ObjectState IdStateNavigation { get; set; }
        public virtual IList<FundPremiseAssoc> FundsPremisesAssoc { get; set; }
        public virtual IList<OwnerPremiseAssoc> OwnerPremisesAssoc { get; set; }
        public virtual IList<OwnershipPremiseAssoc> OwnershipPremisesAssoc { get; set; }
        public virtual IList<SubPremise> SubPremises { get; set; }

        public string GetAddress()
        {
            if (IdPremisesTypeNavigation == null)
                throw new Exception("IdPremisesTypeNavigation не подгружен");
            if (IdBuildingNavigation == null)
                throw new Exception("IdBuildingNavigation не подгружен");
            if (IdBuildingNavigation.IdStreetNavigation == null)
                throw new Exception("IdStreetNavigation не подгружен");
            var address =
                IdBuildingNavigation.IdStreetNavigation.StreetName + ", д." +
                IdBuildingNavigation.House + ", " +
                IdPremisesTypeNavigation.PremisesTypeShort + PremisesNum;
            return address;
        }
    }
}
