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
