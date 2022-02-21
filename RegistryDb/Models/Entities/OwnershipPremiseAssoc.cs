namespace RegistryDb.Models.Entities
{
    public partial class OwnershipPremiseAssoc
    {
        public int IdPremises { get; set; }
        public int IdOwnershipRight { get; set; }
        public byte Deleted { get; set; }

        public virtual OwnershipRight OwnershipRightNavigation { get; set; }
        public virtual Premise PremisesNavigation { get; set; }
    }
}
