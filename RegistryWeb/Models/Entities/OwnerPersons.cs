namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPersons
    {
        public int IdOwnerPersons { get; set; }
        public int IdOwnerProcess { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnerProcesses IdOwnerProcessNavigation { get; set; }
    }
}
