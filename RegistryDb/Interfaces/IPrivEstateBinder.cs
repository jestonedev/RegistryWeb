namespace RegistryDb.Interfaces
{
    public interface IPrivEstateBinder
    {
        string IdStreet { get; set; }
        int? IdBuilding { get; set; }
        int? IdPremise { get; set; }
        int? IdSubPremise { get; set; }
    }
}
