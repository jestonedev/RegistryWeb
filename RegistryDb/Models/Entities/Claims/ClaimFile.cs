namespace RegistryDb.Models.Entities.Claims
{
    public class ClaimFile
    {
        public int IdFile { get; set; }
        public int IdClaim { get; set; }
        public string FileName { get; set; }
        public string DisplayName { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }

        public virtual Claim IdClaimNavigation { get; set; }
    }
}
