namespace RegistryDb.Models.Entities.Claims
{
    public class ClaimStateFile
    {
        public int IdFile { get; set; }
        public int IdState { get; set; }
        public string FileName { get; set; }
        public string DisplayName { get; set; }
        public string MimeType { get; set; }
        public virtual ClaimState IdClaimStateNavigation { get; set; }
    }
}
