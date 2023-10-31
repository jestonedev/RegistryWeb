using RegistryDb.Models;
using RegistryDb.Models.Entities.KumiAccounts;
using System.Collections.Generic;
using System.Linq;

namespace RegistryServices.DataServices.KumiPayments
{
    public class KumiUntiedPaymentsService
    {
        private readonly RegistryContext registryContext;

        public KumiUntiedPaymentsService(RegistryContext registryContext) {
            this.registryContext = registryContext;
        }

        public IList<KumiPaymentUntied> GetUntiedPayments(int? idAccount, int? idClaim, int? idCharge)
        {
            if (idCharge != null)
            {
                return registryContext.KumiPaymentsUntied
                    .Where(r => r.IdCharge == idCharge).ToList();
            }
            if (idClaim != null)
            {
                return registryContext.KumiPaymentsUntied
                    .Where(r => r.IdClaim != null && r.IdClaim == idClaim).ToList();
            }
            if (idAccount != null)
            {
                var idCharges = registryContext.KumiCharges.Where(r => r.IdAccount == idAccount).Select(r => r.IdCharge).ToList();
                var idClaims = registryContext.Claims.Where(r => r.IdAccountKumi != null && r.IdAccountKumi == idAccount)
                    .Select(r => r.IdClaim).ToList();

                return registryContext.KumiPaymentsUntied.Where(r => idCharges.Contains(r.IdCharge))
                    .Union(
                        registryContext.KumiPaymentsUntied.Where(r => r.IdClaim != null && idClaims.Contains(r.IdClaim.Value))
                    ).Distinct().ToList();
            }
            return new List<KumiPaymentUntied>();
        }
    }
}
