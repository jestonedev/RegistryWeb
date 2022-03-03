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
using System.Globalization;

namespace RegistryWeb.DataServices
{
    public class PrivatizationReportsDataService
    {
        private readonly RegistryContext registryContext;
        private readonly SecurityService securityServices;

        public PrivatizationReportsDataService(RegistryContext registryContext, SecurityService securityServices)
        {
            this.registryContext = registryContext;
            this.securityServices = securityServices;
        }

        public PrivatizationReportsVM GetViewModel()
        {
            var viewModel = new PrivatizationReportsVM
            {
                Executors = registryContext.Executors.Where(e => !e.IsInactive).ToList(),
                Regions = registryContext.KladrRegions.ToList(),
                Streets = registryContext.KladrStreets.ToList(),
                LiteraTypes = new List<KeyValuePair<int, string>>() {
                    new KeyValuePair<int, string>(0, "С литерой \"п\""),
                    new KeyValuePair<int, string>(1, "Без литеры \"п\"")
                },
                Order = new List<KeyValuePair<int, string>>() {
                    new KeyValuePair<int, string>(0, "По дате"),
                    new KeyValuePair<int, string>(1, "По рег. номеру")
                },
                PremiseTypes = new List<KeyValuePair<int, string>>() {
                    new KeyValuePair<int, string>(0, "Квартира"),
                    new KeyValuePair<int, string>(1, "Комната"),
                    new KeyValuePair<int, string>(2, "Квартира с подселением"),
                    new KeyValuePair<int, string>(3, "Жилой дом")
                }
            };

            var monthsList = DateTimeFormatInfo.CurrentInfo.MonthNames.Take(12).ToList();

            viewModel.MonthsList = new Dictionary<int, string>();
            for (var i = 0; i < monthsList.Count(); i++)
                viewModel.MonthsList.Add(i + 1, monthsList[i]);

            return viewModel;
        }
    }
}
