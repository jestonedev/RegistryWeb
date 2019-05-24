using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPersons
    {
        public int IdOwnerPersons { get; set; }
        public int IdOwnerProcess { get; set; }
        public int IdKinship { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerProcesses IdOwnerProcessNavigation { get; set; }
    }
}
