using RegistryDb.Models.Entities;

namespace RegistryDb.Models.SqlViews
{
    public class OwnerActiveProcess
    {
        public int IdProcess { get; set; }
        public int? IdBuilding { get; set; }
        public int? IdPremise { get; set; }
        public int? IdSubPremise { get; set; }
        public string Owners { get; set; }
        public int CountOwners { get; set; }

        public virtual OwnerProcess OwnerProcessNavigation { get; set; }
    }
}
