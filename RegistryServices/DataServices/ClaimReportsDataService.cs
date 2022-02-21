using RegistryDb.Models;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.DataServices
{
    public class ClaimReportsDataService
    {
        private readonly RegistryContext registryContext;
        private readonly SecurityService securityServices;

        public ClaimReportsDataService(RegistryContext registryContext, SecurityService securityServices)
        {
            this.registryContext = registryContext;
            this.securityServices = securityServices;
        }

        public bool HasClaimState(int idClaim, int idStateType)
        {
            return registryContext.ClaimStates.FirstOrDefault(r => r.IdClaim == idClaim && r.IdStateType == idStateType) != null;
        }

        public List<int> GetAccountIds(List<int> idClaims)
        {
            return registryContext.Claims.Where(r => idClaims.Contains(r.IdClaim) && r.IdAccount != null).Select(r => r.IdAccount.Value).Distinct().ToList();
        }

        public ClaimReportsVM GetViewModel()
        {
            var viewModel = new ClaimReportsVM
            {
                Executors = registryContext.Executors.Where(e => !e.IsInactive).ToList(),
                StateTypes = registryContext.ClaimStateTypes.ToList(),
                CurrentExecutor = securityServices.Executor
            };

            var monthsList = registryContext.Payments
                            .Select(p => p.Date).Distinct()
                            .OrderByDescending(p => p.Date).Take(6)
                            .ToList();

            viewModel.MonthsList = new Dictionary<int, DateTime>();
            for (var i = 0; i < monthsList.Count(); i++)
                viewModel.MonthsList.Add(monthsList[i].Month, monthsList[i].Date);


            return viewModel;
        }

        public string GetExecutor(int idExecutor)
        {
            return registryContext.Executors.FirstOrDefault(r => r.IdExecutor == idExecutor)?.ExecutorName;
        }
    }
}
