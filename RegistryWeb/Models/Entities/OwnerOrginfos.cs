namespace RegistryWeb.Models.Entities
{
    public partial class OwnerOrginfos
    {
        public int IdOrginfo { get; set; }
        public int IdProcess { get; set; }
        public string OrgName { get; set; }
        public int NumeratorShare { get; set; }
        public int DenominatorShare { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerProcesses IdOwnerProcessNavigation { get; set; }
    }
}
