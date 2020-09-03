using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        internal bool HasClaimState(int idClaim, int idStateType)
        {
            return registryContext.ClaimStates.FirstOrDefault(r => r.IdClaim == idClaim && r.IdStateType == idStateType) != null;
        }

        internal List<int> GetAccountIds(List<int> idClaims)
        {
            return registryContext.Claims.Where(r => idClaims.Contains(r.IdClaim)).Select(r => r.IdAccount).Distinct().ToList();
        }

        public ClaimReportsVM GetViewModel()
        {
            var viewModel = new ClaimReportsVM
            {
                Executors = registryContext.Executors.Where(e => !e.IsInactive).ToList(),
                StateTypes = registryContext.ClaimStateTypes.ToList(),
                CurrentExecutor = securityServices.Executor
            };
            return viewModel;
        }

        internal string GetExecutor(int idExecutor)
        {
            return registryContext.Executors.FirstOrDefault(r => r.IdExecutor == idExecutor)?.ExecutorName;
        }
    }
}
