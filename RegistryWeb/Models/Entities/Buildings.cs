using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class Buildings
    {
        public Buildings()
        {
            FundsBuildingsAssoc = new List<FundsBuildingsAssoc>();
            OwnerBuildingsAssoc = new List<OwnerBuildingsAssoc>();
            OwnershipBuildingsAssoc = new List<OwnershipBuildingsAssoc>();
            Premises = new List<Premises>();
        }

        public int IdBuilding { get; set; }
        public int IdState { get; set; }
        public int IdStructureType { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public short Floors { get; set; }
        public int NumPremises { get; set; }
        public int NumRooms { get; set; }
        public int NumApartments { get; set; }
        public int NumSharedApartments { get; set; }
        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
        public string CadastralNum { get; set; }
        public decimal CadastralCost { get; set; }
        public decimal BalanceCost { get; set; }
        public int StartupYear { get; set; }
        public byte Improvement { get; set; }
        public byte Elevator { get; set; }
        public byte? RubbishChute { get; set; }
        public double? Wear { get; set; }
        public string Description { get; set; }
        public DateTime? StateDate { get; set; }
        public byte? Plumbing { get; set; }
        public byte? HotWaterSupply { get; set; }
        public byte? Canalization { get; set; }
        public byte? Electricity { get; set; }
        public byte? RadioNetwork { get; set; }
        public int? IdHeatingType { get; set; }
        public string BtiRooms { get; set; }
        public string HousingCooperative { get; set; }
        public DateTime RegDate { get; set; }
        public decimal RentCoefficient { get; set; }
        public byte IsMemorial { get; set; }
        public DateTime? DemolishedFactDate { get; set; }
        public string LandCadastralNum { get; set; }
        public DateTime? LandCadastralDate { get; set; }
        public double LandArea { get; set; }
        public byte Deleted { get; set; }

        public virtual HeatingType IdHeatingTypeNavigation { get; set; }
        public virtual ObjectStates IdStateNavigation { get; set; }
        public virtual StructureTypes IdStructureTypeNavigation { get; set; }
        public virtual KladrStreets IdStreetNavigation { get; set; }
        public virtual IList<FundsBuildingsAssoc> FundsBuildingsAssoc { get; set; }
        public virtual IList<OwnerBuildingsAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual IList<OwnershipBuildingsAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual IList<Premises> Premises { get; set; }
    }
}
