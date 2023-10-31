using Microsoft.Extensions.DependencyInjection;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryDb.Models.Entities.Tenancies;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.DataFilterServices
{
    public class FilterServiceFactory<T>
        where T : IFilterServiceCommon
    {
        private readonly IServiceProvider provider;

        public FilterServiceFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }
        public T CreateInstance() {
            var serviceType = GetType().GenericTypeArguments[0];
            if (serviceType.IsGenericType)
            {
                var entityType = serviceType.GenericTypeArguments[0];
                if (entityType == typeof(Building))
                    return (T)ActivatorUtilities.CreateInstance(provider, typeof(BuildingsFilterService));
                if (entityType == typeof(Premise))
                    return (T)ActivatorUtilities.CreateInstance(provider, typeof(PremisesFilterService));
                if (entityType == typeof(TenancyProcess))
                    return (T)ActivatorUtilities.CreateInstance(provider, typeof(TenancyProcessFilterService));
                if (entityType == typeof(DocumentIssuedBy))
                    return (T)ActivatorUtilities.CreateInstance(provider, typeof(DocumentIssuedByFilterService));
                if (entityType == typeof(KumiPayment))
                    return (T)ActivatorUtilities.CreateInstance(provider, typeof(KumiPaymentsFilterService));
            }
            throw new ArgumentException("Не удалось инициализировать класс, реализующий IFilterSerivce<>");
        }
    }
}
