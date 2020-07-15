using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class Building : IAddress
    {
        public Building()
        {
            FundsBuildingsAssoc = new List<FundBuildingAssoc>();
            OwnerBuildingsAssoc = new List<OwnerBuildingAssoc>();
            OwnershipBuildingsAssoc = new List<OwnershipBuildingAssoc>();
            RestrictionBuildingsAssoc = new List<RestrictionBuildingAssoc>();
            Premises = new List<Premise>();
            TenancyBuildingsAssoc = new List<TenancyBuildingAssoc>();
            BuildingDemolitionActFiles = new List<BuildingDemolitionActFile>();
        }

        public int IdBuilding { get; set; }
        [Required]
        public int IdState { get; set; }
        [Required]
        public int IdStructureType { get; set; }
        [Required]
        public int IdStructureTypeOverlap { get; set; }
        [Required]
        public string IdStreet { get; set; }
        [Required]
        public string House { get; set; }
        [Required]
        public short Floors { get; set; }
        public short? Entrances { get; set; }
        [Required]
        public int NumPremises { get; set; }
        [Required]
        public int NumRooms { get; set; }
        [Required]
        public int NumApartments { get; set; }
        [Required]
        public int NumSharedApartments { get; set; }
        [Required]
        public double TotalArea { get; set; }
        [Required]
        public double LivingArea { get; set; }
        [Required]
        public double UnlivingArea { get; set; }
        [Required]
        public double CommonPropertyArea { get; set; }
        public string CadastralNum { get; set; }
        [Required]
        public decimal CadastralCost { get; set; }
        [Required]
        public decimal BalanceCost { get; set; }
        [Required]
        public int StartupYear { get; set; }
        public string Series { get; set; }
        [Required]
        public bool Improvement { get; set; }
        [Required]
        public bool Elevator { get; set; }
        public bool? RubbishChute { get; set; }
        public double? Wear { get; set; }
        public string Description { get; set; }
        //Не используется
        //public DateTime? StateDate { get; set; }
        public bool? Plumbing { get; set; }
        public bool? HotWaterSupply { get; set; }
        public bool? Canalization { get; set; }
        public bool? Electricity { get; set; }
        public bool? RadioNetwork { get; set; }
        public int? IdHeatingType { get; set; }
        [Required]
        public int IdDecree { get; set; }
        public string BtiRooms { get; set; }
        public string HousingCooperative { get; set; }
        [Required]
        public DateTime RegDate { get; set; }
        [Required]
        public decimal RentCoefficient { get; set; }
        [Required]
        public bool IsMemorial { get; set; }
        public DateTime? MemorialDate { get; set; }
        public string MemorialNumber { get; set; }
        public string MemorialNameOrg { get; set; }
        public DateTime? DateOwnerEmergency { get; set; }
        public DateTime? DemolishedFactDate { get; set; }
        public DateTime? DemolishedPlanDate { get; set; }
        public string LandCadastralNum { get; set; }
        public DateTime? LandCadastralDate { get; set; }
        [Required]
        public double LandArea { get; set; }
        public byte Deleted { get; set; }

        public virtual HeatingType IdHeatingTypeNavigation { get; set; }
        public virtual ObjectState IdStateNavigation { get; set; }
        public virtual StructureType IdStructureTypeNavigation { get; set; }
        public virtual StructureTypeOverlap StructureTypeOverlapNavigation { get; set; }
        public virtual GovernmentDecree GovernmentDecreeNavigation { get; set; }

        public virtual KladrStreet IdStreetNavigation { get; set; }
        public virtual IList<FundBuildingAssoc> FundsBuildingsAssoc { get; set; }
        public virtual IList<OwnerBuildingAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual IList<OwnershipBuildingAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual IList<RestrictionBuildingAssoc> RestrictionBuildingsAssoc { get; set; }
        public virtual IList<Premise> Premises { get; set; }
        public virtual IList<TenancyBuildingAssoc> TenancyBuildingsAssoc { get; set; }
        public virtual IList<BuildingDemolitionActFile> BuildingDemolitionActFiles { get; set; }

        public string GetAddress()
        {
            if (IdStreetNavigation == null)
                throw new Exception("IdStreetNavigation не подгружен");
            var address = IdStreetNavigation.StreetName + ", д." + House;
            return address;
        }
    }
}
