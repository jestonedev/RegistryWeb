using RegistryDb.Models;
using RegistryServices.ViewModel.Claims;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.DataServices.Claims
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

        public Tuple<List<int>, List<int>> GetAccountIds(List<int> idClaims)
        {
            var idAccountsBks = registryContext.Claims
                .Where(r => idClaims.Contains(r.IdClaim) && r.IdAccount != null)
                .Select(r => r.IdAccount.Value).Distinct().ToList();
            var idAccountsKumi = registryContext.Claims
                .Where(r => idClaims.Contains(r.IdClaim) && r.IdAccountKumi != null)
                .Select(r => r.IdAccountKumi.Value).Distinct().ToList();
            return new Tuple<List<int>, List<int>>(idAccountsBks, idAccountsKumi);
        }

        public ClaimReportsVM GetViewModel()
        {
            var viewModel = new ClaimReportsVM
            {
                Executors = registryContext.Executors.Where(e => !e.IsInactive).ToList(),
                StateTypes = registryContext.ClaimStateTypes.ToList(),
                CurrentExecutor = securityServices.Executor,
                BuildingManagmentOrgs = registryContext.BuildingManagmentOrgs.ToList()
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
