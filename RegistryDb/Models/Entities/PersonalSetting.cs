namespace RegistryDb.Models.Entities
{
    public class PersonalSetting
    {
        public int IdUser { get; set; }
        public string PaymentAccauntTableJson { get; set; }

        public virtual AclUser AclUser { get; set; }

    }
}
