﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class Premise : IAddress
    {
        public Premise()
        {
            FundsPremisesAssoc = new List<FundPremiseAssoc>();
            OwnerPremisesAssoc = new List<OwnerPremiseAssoc>();
            OwnershipPremisesAssoc = new List<OwnershipPremiseAssoc>();
            //RestrictionPremisesAssoc = new List<RestrictionpPremiseAssoc>();
            SubPremises = new List<SubPremise>();
            TenancyPremisesAssoc = new List<TenancyPremiseAssoc>();
        }

        public int IdPremises { get; set; }
        [Required(ErrorMessage = "Выберете здание")]
        public int IdBuilding { get; set; }
        [Required(ErrorMessage = "Выберете состояние помещения")]
        public int IdState { get; set; }        
        public int IdPremisesKind { get; set; }
        [Required(ErrorMessage = "Выберете тип помещения")]
        public int IdPremisesType { get; set; }
        [Required(ErrorMessage = "Выберете примечание")]
        public int IdPremisesComment { get; set; }
        [Required(ErrorMessage = "Выберете местонахождение ключей")]
        public int IdPremisesDoorKeys { get; set; }
        [Required(ErrorMessage = "Укажите номер помещения")]
        public string PremisesNum { get; set; }
        //[Required(ErrorMessage = "Укажите этаж")]
        [Range(-1, Double.MaxValue, ErrorMessage = "Количество материала не может быть меньше нуля")]
        public short Floor { get; set; }
        //[Required(ErrorMessage = "Укажите количество комнат")]
        [Range(-1, Double.MaxValue, ErrorMessage = "Количество материала не может быть меньше нуля")]
        public short NumRooms { get; set; }
        //[Required(ErrorMessage = "Укажите количество койко-мест")]
        [Range(-1, Double.MaxValue, ErrorMessage = "Количество материала не может быть меньше нуля")]
        public short NumBeds { get; set; }
        [Required(ErrorMessage = "Укажите общую площадь")]
        public double TotalArea { get; set; }
        [Required(ErrorMessage = "Укажите жилую площадь")]
        public double LivingArea { get; set; }
        [Required(ErrorMessage = "Укажите высоту помещения")]
        public double Height { get; set; }
        public string CadastralNum { get; set; }
        [Required(ErrorMessage = "Укажите кадастровую стоимость")]
        [Range(-1, Double.MaxValue, ErrorMessage = "Количество материала не может быть меньше нуля")]
        public decimal CadastralCost { get; set; }
        [Required(ErrorMessage = "Укажите балансовую стоимость")]
        [Range(-1, Double.MaxValue, ErrorMessage = "Количество материала не может быть меньше нуля")]
        public decimal BalanceCost { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Укажите дату внесения в реестр")]
        public DateTime RegDate { get; set; }
        public byte IsMemorial { get; set; }
        public string Account { get; set; }
        public DateTime? StateDate { get; set; }
        public byte Deleted { get; set; }

        public virtual Building IdBuildingNavigation { get; set; }
        public virtual RentPremise IdRentPremiseNavigation { get; set; }
        public virtual PremisesComment IdPremisesCommentNavigation { get; set; }
        public virtual PremisesKind IdPremisesKindNavigation { get; set; }
        public virtual PremisesType IdPremisesTypeNavigation { get; set; }
        public virtual PremisesDoorKeys IdPremisesDoorKeysNavigation { get; set; }
        public virtual ObjectState IdStateNavigation { get; set; }
        public virtual IList<FundPremiseAssoc> FundsPremisesAssoc { get; set; }
        public virtual IList<OwnerPremiseAssoc> OwnerPremisesAssoc { get; set; }
        public virtual IList<OwnershipPremiseAssoc> OwnershipPremisesAssoc { get; set; }
        public virtual IList<RestrictionPremiseAssoc> RestrictionPremisesAssoc { get; set; }
        public virtual IList<SubPremise> SubPremises { get; set; }
        public IList<TenancyPremiseAssoc> TenancyPremisesAssoc { get; set; }

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
