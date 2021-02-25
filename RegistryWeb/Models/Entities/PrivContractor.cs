using System;

namespace RegistryWeb.Models.Entities
{
    public class PrivContractor
    {
        public int IdContractor { get; set; }
        public bool IsNonontractor { get; set; }
        public int IdContract { get; set; }
        public DateTime AgreementDate { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public int IdKinship { get; set; }
        public DateTime DateBirth { get; set; }
        public string Passport { get; set; }
        public string Part { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
    }
}