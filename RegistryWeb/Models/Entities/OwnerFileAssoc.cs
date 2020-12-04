namespace RegistryWeb.Models.Entities
{
    public partial class OwnerFileAssoc
    {
        public int Id { get; set; }
        public int IdOwner { get; set; }
        public int IdFile { get; set; }
        public int NumeratorShare { get; set; }
        public int DenominatorShare { get; set; }
        public byte Deleted { get; set; }

        public virtual Owner Owner { get; set; }
        public virtual OwnerFile OwnerFile { get; set; }
    }
}