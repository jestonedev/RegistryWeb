using RegistryDb.Interfaces;
using RegistryDb.Models.Entities.Owners;
using RegistryDb.Models.Entities.Payments;
using RegistryDb.Models.Entities.Privatization;
using RegistryDb.Models.Entities.RegistryObjects.Common;
using RegistryDb.Models.Entities.RegistryObjects.Common.Funds;
using RegistryDb.Models.Entities.RegistryObjects.Common.Resettle;
using RegistryDb.Models.Entities.Tenancies;
using System;
using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Premises
{
    public partial class SubPremise : IAddress
    {
        public SubPremise()
        {
            PrivContracts = new List<PrivContract>();
            FundsSubPremisesAssoc = new List<FundSubPremiseAssoc>();
            OwnerSubPremisesAssoc = new List<OwnerSubPremiseAssoc>();
            TenancySubPremisesAssoc = new List<TenancySubPremiseAssoc>();
            PaymentAccountSubPremisesAssoc = new List<PaymentAccountSubPremiseAssoc>();
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

        public virtual Premise IdPremisesNavigation { get; set; }
        public virtual ObjectState IdStateNavigation { get; set; }
        public virtual IList<PrivContract> PrivContracts { get; set; }
        public virtual IList<FundSubPremiseAssoc> FundsSubPremisesAssoc { get; set; }
        public virtual IList<OwnerSubPremiseAssoc> OwnerSubPremisesAssoc { get; set; }
        public IList<TenancySubPremiseAssoc> TenancySubPremisesAssoc { get; set; }
        public virtual IList<ResettleInfoSubPremiseFrom> ResettleInfoSubPremisesFrom { get; set; }
        public virtual IList<PaymentAccountSubPremiseAssoc> PaymentAccountSubPremisesAssoc { get; set; }
        public virtual IList<TenancyPaymentHistory> TenancyPaymentsHistory { get; set; }

        public string GetAddress()
        {
            if (IdPremisesNavigation == null)
                throw new Exception("IdPremisesNavigation не подгружен");
            if (IdPremisesNavigation.IdPremisesTypeNavigation == null)
                throw new Exception("IdPremisesTypeNavigation не подгружен");
            if (IdPremisesNavigation.IdBuildingNavigation == null)
                throw new Exception("IdBuildingNavigation не подгружен");
            if (IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation == null)
                throw new Exception("IdStreetNavigation не подгружен");
            var address =
                IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName +
                ", д." + IdPremisesNavigation.IdBuildingNavigation.House + ", " +
                IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                IdPremisesNavigation.PremisesNum + ", к." + SubPremisesNum;
            return address;
        }
    }
}