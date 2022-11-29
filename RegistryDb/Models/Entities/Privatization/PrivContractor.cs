using RegistryDb.Models.Entities.Tenancies;
using System;

namespace RegistryDb.Models.Entities.Privatization
{
    public class PrivContractor
    {
        public int IdContractor { get; set; }
        public bool IsNoncontractor { get; set; }
        public int IdContract { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public int IdKinship { get; set; }
        public DateTime? DateBirth { get; set; }
        public bool? HasDover { get; set; }
        public string Description { get; set; }
        public string Passport { get; set; }
        public string Part { get; set; }        
        public bool Deleted { get; set; }

        public virtual PrivContract PrivContractNavigation { get; set; }
        public virtual Kinship KinshipNavigation { get; set; }
    }
}