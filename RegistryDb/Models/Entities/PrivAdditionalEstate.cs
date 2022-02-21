using RegistryDb.Interfaces;
using System;

namespace RegistryDb.Models.Entities
{
    public class PrivAdditionalEstate : IPrivEstateBinder
    {
        public int IdEstate { get; set; }
        public int IdContract { get; set; }
        public string IdStreet { get; set; }
        public int? IdBuilding { get; set; }
        public int? IdPremise { get; set; }
        public int? IdSubPremise { get; set; }
        public bool Deleted { get; set; }
        public virtual PrivContract PrivContractNavigation { get; set; }
    }
}