namespace RegistryDb.Models.Entities.Owners.Log
{
    public class LogOwnerProcessValue
    {
        public int Id { get; set; }
        public int IdLog { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }

        public virtual LogOwnerProcess IdLogNavigation { get; set; }
    }
}
