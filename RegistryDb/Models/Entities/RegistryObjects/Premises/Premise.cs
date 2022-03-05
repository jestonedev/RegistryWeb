using RegistryDb.Interfaces;
using RegistryDb.Models.Entities.Owners;
using RegistryDb.Models.Entities.Payments;
using RegistryDb.Models.Entities.Privatization;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryDb.Models.Entities.RegistryObjects.Buildings.Litigations;
using RegistryDb.Models.Entities.RegistryObjects.Common;
using RegistryDb.Models.Entities.RegistryObjects.Common.Funds;
using RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships;
using RegistryDb.Models.Entities.RegistryObjects.Common.Resettle;
using RegistryDb.Models.Entities.RegistryObjects.Common.Restrictions;
using RegistryDb.Models.Entities.Tenancies;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities.RegistryObjects.Premises
{
    public partial class Premise : IAddress
    {
        public Premise()
        {
            PrivContracts = new List<PrivContract>();
            FundsPremisesAssoc = new List<FundPremiseAssoc>();
            OwnerPremisesAssoc = new List<OwnerPremiseAssoc>();
            OwnershipPremisesAssoc = new List<OwnershipPremiseAssoc>();
            RestrictionPremisesAssoc = new List<RestrictionPremiseAssoc>();
            SubPremises = new List<SubPremise>();
            TenancyPremisesAssoc = new List<TenancyPremiseAssoc>();
            ResettlePremisesAssoc = new List<ResettlePremiseAssoc>();
            LitigationPremisesAssoc = new List<LitigationPremiseAssoc>();
            PaymentAccountPremisesAssoc = new List<PaymentAccountPremiseAssoc>();
        }

        public int IdPremises { get; set; }
        [Required(ErrorMessage = "Выберите здание")]
        public int IdBuilding { get; set; }
        [Required(ErrorMessage = "Выберите состояние помещения")]
        public int IdState { get; set; }        
        public int IdPremisesKind { get; set; }
        [Required(ErrorMessage = "Выберите тип помещения")]
        public int IdPremisesType { get; set; }
        [Required(ErrorMessage = "Выберите примечание")]
        public int IdPremisesComment { get; set; }
        [Required(ErrorMessage = "Выберите местонахождение ключей")]
        public int IdPremisesDoorKeys { get; set; }
        [Required(ErrorMessage = "Укажите номер помещения")]
        public string PremisesNum { get; set; }
        [Required(ErrorMessage = "Укажите этаж", AllowEmptyStrings = false)]
        [Range(0, Double.MaxValue, ErrorMessage = "Этаж не может быть меньше нуля")]
        public short Floor { get; set; }
        [Required(ErrorMessage = "Укажите количество комнат", AllowEmptyStrings = false)]
        [Range(0, Double.MaxValue, ErrorMessage = "Количество комнат не может быть меньше нуля")]
        public short NumRooms { get; set; }
        [Required(ErrorMessage = "Укажите количество койко-мест", AllowEmptyStrings = false)]
        [Range(0, Double.MaxValue, ErrorMessage = "Количество койко-мест не может быть меньше нуля")]
        public short NumBeds { get; set; }
        [Required(ErrorMessage = "Укажите общую площадь", AllowEmptyStrings = false)]
        public double TotalArea { get; set; }
        [Required(ErrorMessage = "Укажите жилую площадь", AllowEmptyStrings = false)]
        public double LivingArea { get; set; }
        [Required(ErrorMessage = "Укажите высоту помещения", AllowEmptyStrings = false)]
        public double Height { get; set; }
        public string CadastralNum { get; set; }
        [Required(ErrorMessage = "Укажите кадастровую стоимость", AllowEmptyStrings = false)]
        [Range(0, Double.MaxValue, ErrorMessage = "Кадастровая стоимость должна быть больше нуля")]
        public decimal CadastralCost { get; set; }
        [Required(ErrorMessage = "Укажите балансовую стоимость", AllowEmptyStrings = false)]
        [Range(-1, Double.MaxValue, ErrorMessage = "Балансовая стоимость должна быть больше нуля")]
        public decimal BalanceCost { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Укажите дату включения в РМИ")]
        public DateTime RegDate { get; set; }
        public byte IsMemorial { get; set; }
        public string Account { get; set; }
        public DateTime? StateDate { get; set; }
        public byte Deleted { get; set; }

        public virtual Building IdBuildingNavigation { get; set; }
        public virtual PremisesComment IdPremisesCommentNavigation { get; set; }
        public virtual PremisesKind IdPremisesKindNavigation { get; set; }
        public virtual PremisesType IdPremisesTypeNavigation { get; set; }
        public virtual PremisesDoorKeys IdPremisesDoorKeysNavigation { get; set; }
        public virtual ObjectState IdStateNavigation { get; set; }
        public virtual IList<PrivContract> PrivContracts { get; set; }
        public virtual IList<FundPremiseAssoc> FundsPremisesAssoc { get; set; }
        public virtual IList<OwnerPremiseAssoc> OwnerPremisesAssoc { get; set; }
        public virtual IList<OwnershipPremiseAssoc> OwnershipPremisesAssoc { get; set; }
        public virtual IList<RestrictionPremiseAssoc> RestrictionPremisesAssoc { get; set; }
        public virtual IList<LitigationPremiseAssoc> LitigationPremisesAssoc { get; set; }
        public virtual IList<ResettlePremiseAssoc> ResettlePremisesAssoc { get; set; }
        public virtual IList<SubPremise> SubPremises { get; set; }
        public IList<TenancyPremiseAssoc> TenancyPremisesAssoc { get; set; }
        public virtual IList<PaymentAccountPremiseAssoc> PaymentAccountPremisesAssoc { get; set; }
        public virtual IList<TenancyPaymentHistory> TenancyPaymentsHistory { get; set; }

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
