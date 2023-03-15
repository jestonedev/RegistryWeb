using RegistryDb.Interfaces;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Owners;
using RegistryDb.Models.Entities.Privatization;
using RegistryDb.Models.Entities.RegistryObjects.Common;
using RegistryDb.Models.Entities.RegistryObjects.Common.Funds;
using RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships;
using RegistryDb.Models.Entities.RegistryObjects.Common.Restrictions;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryDb.Models.Entities.Tenancies;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities.RegistryObjects.Buildings
{
    public partial class Building : IAddress
    {
        public Building()
        {
            PrivContracts = new List<PrivContract>();
            FundsBuildingsAssoc = new List<FundBuildingAssoc>();
            OwnerBuildingsAssoc = new List<OwnerBuildingAssoc>();
            OwnershipBuildingsAssoc = new List<OwnershipBuildingAssoc>();
            RestrictionBuildingsAssoc = new List<RestrictionBuildingAssoc>();
            Premises = new List<Premise>();
            TenancyBuildingsAssoc = new List<TenancyBuildingAssoc>();
            BuildingDemolitionActFiles = new List<BuildingDemolitionActFile>();
            JudgeBuildingsAssoc = new List<JudgeBuildingAssoc>();
        }

        public int IdBuilding { get; set; }
        [Required(ErrorMessage = "Выберите состояние здания")]
        public int IdState { get; set; }
        [Required(ErrorMessage = "Выберите тип строения")]
        public int IdStructureType { get; set; }
        [Required(ErrorMessage = "Выберите тип перекрытия")]
        public int IdStructureTypeOverlap { get; set; }
        [Required(ErrorMessage = "Выберите тип фундамента")]
        public int IdFoundationType { get; set; }
        [Required(ErrorMessage = "Выберите улицу")]
        public string IdStreet { get; set; }
        [Required(ErrorMessage = "Укажите номер дома")]
        public string House { get; set; }
        [Required(ErrorMessage = "Укажите этажность")]
        public short Floors { get; set; }
        public short? Entrances { get; set; }
        [Required(ErrorMessage = "Укажите кол-во помещений")]
        public int NumPremises { get; set; }
        [Required(ErrorMessage = "Укажите кол-во комнат")]
        public int NumRooms { get; set; }
        [Required(ErrorMessage = "Укажите кол-во квартир")]
        public int NumApartments { get; set; }
        [Required(ErrorMessage = "Укажите кол-во квартир с подселением")]
        public int NumSharedApartments { get; set; }
        [Required(ErrorMessage = "Укажите общую площадь")]
        public double TotalArea { get; set; }
        [Required(ErrorMessage = "Укажите жилую площадь")]
        public double LivingArea { get; set; }
        [Required(ErrorMessage = "Укажите нежилую площадь")]
        public double UnlivingArea { get; set; }
        [Required(ErrorMessage = "Укажите площадь общего имущества")]
        public double CommonPropertyArea { get; set; }
        public string CadastralNum { get; set; }
        [Required(ErrorMessage = "Укажите кадастровую стоимость")]
        [Range(0, Double.MaxValue, ErrorMessage = "Кадастровая стоимость должна быть больше нуля")]
        public decimal CadastralCost { get; set; }
        [Required(ErrorMessage = "Укажите балансовую стоимость")]
        [Range(0, Double.MaxValue, ErrorMessage = "Балансовая стоимость должна быть больше нуля")]
        public decimal BalanceCost { get; set; }
        [Required(ErrorMessage = "Укажите год ввода в эксплуатацию")]
        public int StartupYear { get; set; }
        public string Series { get; set; }
        [Required]
        public bool Improvement { get; set; }
        [Required]
        public bool Elevator { get; set; }
        public bool RubbishChute { get; set; }
        public double? Wear { get; set; }
        public string Description { get; set; }
        //Не используется
        //public DateTime? StateDate { get; set; }
        public bool Plumbing { get; set; }
        public bool HotWaterSupply { get; set; }
        public bool Canalization { get; set; }
        public bool Electricity { get; set; }
        public bool RadioNetwork { get; set; }
        public int? IdHeatingType { get; set; }
        [Required(ErrorMessage = "Выберите постановление")]
        public int IdDecree { get; set; }
        public string BtiRooms { get; set; }
        public string HousingCooperative { get; set; }
        [Required(ErrorMessage = "Укажите дату включения в РМИ")]
        public DateTime RegDate { get; set; }
        [Required(ErrorMessage = "Укажите коэффициент оплаты")]
        public decimal RentCoefficient { get; set; }
        [Required]
        public bool IsMemorial { get; set; }
        public DateTime? MemorialDate { get; set; }
        public string MemorialNumber { get; set; }
        public string MemorialNameOrg { get; set; }
        public DateTime? DateOwnerEmergency { get; set; }
        public DateTime? DemolishedFactDate { get; set; }
        public DateTime? DemolishedPlanDate { get; set; }
        public DateTime? DemandForDemolishingDeliveryDate { get; set; }
        public string LandCadastralNum { get; set; }
        public DateTime? LandCadastralDate { get; set; }
        [Required(ErrorMessage = "Укажите площадь земельного участка")]
        public double LandArea { get; set; }
        public int? IdOrganization { get; set; }
        public string PostIndex { get; set; }
        public byte Deleted { get; set; }

        public virtual HeatingType IdHeatingTypeNavigation { get; set; }
        public virtual ObjectState IdStateNavigation { get; set; }
        public virtual StructureType IdStructureTypeNavigation { get; set; }
        public virtual StructureTypeOverlap StructureTypeOverlapNavigation { get; set; }
        public virtual FoundationType FoundationTypeNavigation { get; set; }
        public virtual GovernmentDecree GovernmentDecreeNavigation { get; set; }
        public virtual BuildingManagmentOrg IdOrganizationNavigation { get; set; }
        public virtual KladrStreet IdStreetNavigation { get; set; }
        public virtual IList<PrivContract> PrivContracts { get; set; }
        public virtual IList<FundBuildingAssoc> FundsBuildingsAssoc { get; set; }
        public virtual IList<OwnerBuildingAssoc> OwnerBuildingsAssoc { get; set; }
        public virtual IList<OwnershipBuildingAssoc> OwnershipBuildingsAssoc { get; set; }
        public virtual IList<RestrictionBuildingAssoc> RestrictionBuildingsAssoc { get; set; }
        public virtual IList<Premise> Premises { get; set; }
        public virtual IList<TenancyBuildingAssoc> TenancyBuildingsAssoc { get; set; }
        public virtual IList<BuildingDemolitionActFile> BuildingDemolitionActFiles { get; set; }
        public virtual IList<BuildingAttachmentFileAssoc> BuildingAttachmentFilesAssoc { get; set; }
        public virtual IList<JudgeBuildingAssoc> JudgeBuildingsAssoc { get; set; }
        public virtual IList<TenancyPaymentHistory> TenancyPaymentsHistory { get; set; }

        public string GetAddress()
        {
            if (IdStreetNavigation == null)
                throw new Exception("IdStreetNavigation не подгружен");
            var address = IdStreetNavigation.StreetName + ", д." + House;
            return address;
        }
    }
}
