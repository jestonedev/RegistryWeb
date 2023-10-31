using Microsoft.AspNetCore.Mvc;
using RegistryDb.Models.Entities.Claims;
using System.Collections.Generic;

namespace RegistryWeb.ViewComponents.KumiAccounts
{
    public class ClaimsTotalComponent: ViewComponent
    {
        public IViewComponentResult Invoke(IEnumerable<Claim> claims, bool hasDgiCharges, bool hasPkkCharges, bool hasPadunCharges)
        {
            var totals = new decimal[15];
            foreach (var claim in claims)
            {
                totals[0] += claim.AmountTenancy ?? 0m;
                totals[1] += claim.AmountPenalties ?? 0m;
                totals[2] += claim.AmountTenancyRecovered ?? 0m;
                totals[3] += claim.AmountPenaltiesRecovered ?? 0m;
                totals[4] += (claim.AmountTenancy - claim.AmountTenancyRecovered) ?? 0m;
                totals[5] += (claim.AmountPenalties - claim.AmountPenaltiesRecovered) ?? 0m;
                totals[6] += claim.AmountDgi ?? 0m;
                totals[7] += claim.AmountDgiRecovered ?? 0m;
                totals[8] += (claim.AmountDgi - claim.AmountDgiRecovered) ?? 0m;
                totals[9] += claim.AmountPkk ?? 0m;
                totals[10] += claim.AmountPkkRecovered ?? 0m;
                totals[12] += claim.AmountPadun ?? 0m;
                totals[11] += (claim.AmountPkk - claim.AmountPkkRecovered) ?? 0m;
                totals[13] += claim.AmountPadunRecovered ?? 0m;
                totals[14] += (claim.AmountPadun - claim.AmountPadunRecovered) ?? 0m;
            }
            ViewBag.HasDgiCharges = hasDgiCharges;
            ViewBag.HasPkkCharges = hasPkkCharges;
            ViewBag.HasPadunCharges = hasPadunCharges;
            return View(totals);
        }
    }
}
