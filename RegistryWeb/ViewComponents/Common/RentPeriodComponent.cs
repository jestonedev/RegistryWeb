using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewComponents.Common
{
    public class RentPeriodComponent: ViewComponent
    {
        public string Invoke(DateTime? from, DateTime? to)
        {
            var rentPeriod = "";
            if (from != null)
            {
                rentPeriod += "с " + from.Value.ToString("dd.MM.yyyy");
            }
            if (from != null && to != null)
            {
                rentPeriod += " ";
            }
            if (to != null)
            {
                rentPeriod += "по " + to.Value.ToString("dd.MM.yyyy");
            }
            if (rentPeriod == "")
            {
                rentPeriod = "н/а";
            }
            return rentPeriod;
        }
    }
}
