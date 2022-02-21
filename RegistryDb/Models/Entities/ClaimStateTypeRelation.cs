namespace RegistryDb.Models.Entities
{
    public class ClaimStateTypeRelation
    {
        public ClaimStateTypeRelation()
        {
        }

        public int IdRelation { get; set; }
        public int IdStateFrom { get; set; }
        public int IdStateTo { get; set; }
        public byte Deleted { get; set; }
    }
}
