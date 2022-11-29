namespace RegistryDb.Interfaces
{
    public interface IDbConnectionSettings
    {
        string GetConnectionString();
        string GetDbName();
    }
}
