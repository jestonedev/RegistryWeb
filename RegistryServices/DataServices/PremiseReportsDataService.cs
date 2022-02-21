using RegistryWeb.DataHelpers;
using RegistryDb.Models;
using System.Linq;

namespace RegistryWeb.DataServices
{
    public class PremiseReportsDataService
    {
        private readonly RegistryContext registryContext;
        public PremiseReportsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public bool HasMunicipalSubPrmieses(int idPremise)
        {
            var subPremises = registryContext.SubPremises.Where(r => r.IdPremises == idPremise);
            var hasMunSubPremises = subPremises.Any(r => ObjectStateHelper.IsMunicipal(r.IdState));
            return hasMunSubPremises;
        }
    }
}
