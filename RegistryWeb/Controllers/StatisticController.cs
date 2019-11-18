using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Models;
using RegistryWeb.ViewModel;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class StatisticController : Controller
    {
        private RegistryContext registryContext;

        public StatisticController(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        [HttpPost]
        public JsonResult ReestrStatistic()
        {
            var numbers = new int[] { 1, 2, 6, 7 };
            var date = @DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            var mkd = (
                from ttt in (
                    from ggg in (
                        from oba in registryContext.OwnershipBuildingsAssoc
                        join owr in registryContext.OwnershipRights
                            on oba.IdOwnershipRight equals owr.IdOwnershipRight
                        where numbers.Contains(owr.IdOwnershipRightType)
                        orderby oba.IdBuilding ascending, owr.Date descending
                        select new { oba, owr }
                    )
                    group ggg by ggg.oba.IdBuilding into gr
                    select new
                    {
                        oba = gr.Select(g => g.oba).FirstOrDefault(),
                        owr = gr.Select(g => g.owr).FirstOrDefault()
                    }
                )
                where ttt.owr.IdOwnershipRightType == 7
                select ttt
                );
            //var premiseMun = (
                
            //    );
            var countMKD = mkd.Count();
            return Json(new { date, countMKD });
        }
    }
}