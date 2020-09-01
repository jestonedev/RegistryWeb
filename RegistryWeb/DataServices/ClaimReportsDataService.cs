using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
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
        public ClaimReportsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        internal bool HasClaimState(int idClaim, int idStateType)
        {
            return registryContext.ClaimStates.FirstOrDefault(r => r.IdClaim == idClaim && r.IdStateType == idStateType) != null;
        }
    }
}
