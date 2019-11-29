using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RegistryWeb.DataServices;
using RegistryWeb.Models;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.Controllers
{
    [Authorize]
    public class ReestrEmergencyPremisesController : Controller
    {
        private readonly RegistryContext registryContext;
        private readonly SecurityService securityService;

        public ReestrEmergencyPremisesController(RegistryContext registryContext, SecurityService securityService)
        {
            this.registryContext = registryContext;
            this.securityService = securityService;
        }

        public IActionResult Index()
        {
            if (!securityService.HasPrivilege(Privileges.OwnerRead))
                return View("NotAccess");
            return View();
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
            var premiseAndSubPremiseTenancy = (
                from ttt in (
                    from tac in registryContext.TenancyActiveProcesses
                    join tba in registryContext.TenancyBuildingsAssoc
                        on tac.IdProcess equals tba.IdProcess into tbaGr
                    from tbaL in tbaGr.DefaultIfEmpty()
                    join tpa in (
                        from t in registryContext.TenancyPremisesAssoc
                        join p in registryContext.Premises
                            on t.IdPremise equals p.IdPremises
                        select new { t, p }
                        ) on tac.IdProcess equals tpa.t.IdProcess into tpaGr
                    from tpaL in tpaGr.DefaultIfEmpty()
                    join tspa in (
                        from t in registryContext.TenancySubPremisesAssoc
                        join sp in registryContext.SubPremises
                            on t.IdSubPremise equals sp.IdSubPremises
                        join p in registryContext.Premises
                            on sp.IdPremises equals p.IdPremises
                        select new { t, p }
                        ) on tac.IdProcess equals tspa.t.IdProcess into tspaGr
                    from tspaL in tspaGr.DefaultIfEmpty()
                    where tbaL != null || tpaL != null || tspaL != null
                    select new
                    {
                        tac,
                        IdBuilding = tpaL != null ? tpaL.p.IdBuilding : (tspaL != null ? tspaL.p.IdBuilding : tbaL.IdBuilding),
                        TotalArea = tpaL != null ? tpaL.p.TotalArea : (tspaL != null ? tspaL.p.TotalArea : 0),
                        LivingArea = tpaL != null ? tpaL.p.LivingArea : (tspaL != null ? tspaL.p.LivingArea : 0)
                    }
                )
                join m in mkd
                    on ttt.IdBuilding equals m.oba.IdBuilding
                select new { ttt, m }
            );
            var premiseAndSubPremiseOwner = (
                from ttt in (
                    from oap in registryContext.OwnerActiveProcesses
                    join oba in registryContext.OwnerBuildingsAssoc
                        on oap.IdProcess equals oba.IdBuilding into obaGr
                    from obaL in obaGr.DefaultIfEmpty()
                    join opa in (
                        from t in registryContext.OwnerPremisesAssoc
                        join p in registryContext.Premises
                            on t.IdPremise equals p.IdPremises
                        select new { t, p }
                        ) on oap.IdProcess equals opa.t.IdProcess into opaGr
                    from opaL in opaGr.DefaultIfEmpty()
                    join ospa in (
                        from t in registryContext.OwnerSubPremisesAssoc
                        join sp in registryContext.SubPremises
                            on t.IdSubPremise equals sp.IdSubPremises
                        join p in registryContext.Premises
                            on sp.IdPremises equals p.IdPremises
                        select new { t, p }
                        ) on oap.IdProcess equals ospa.t.IdProcess into ospaGr
                    from ospaL in ospaGr.DefaultIfEmpty()
                    where obaL != null || opaL != null || ospaL != null
                    select new
                    {
                        oap,
                        IdBuilding = opaL != null ? opaL.p.IdBuilding : (ospaL != null ? ospaL.p.IdBuilding : obaL.IdBuilding),
                        TotalArea = opaL != null ? opaL.p.TotalArea : (ospaL != null ? ospaL.p.TotalArea : 0),
                        LivingArea = opaL != null ? opaL.p.LivingArea : (ospaL != null ? ospaL.p.LivingArea : 0)
                    }
                )
                join m in mkd
                    on ttt.IdBuilding equals m.oba.IdBuilding
                select new { ttt, m }
            );
            var countMKD = mkd.Count();
            var countTenancy = premiseAndSubPremiseTenancy.Count();
            var countOwner = premiseAndSubPremiseOwner.Count();
            var countInhabitant = 0;
            var totalAreaSum = 0.0;
            var livingAreaSum = 0.0;
            foreach (var ten in premiseAndSubPremiseTenancy)
            {
                countInhabitant += ten.ttt.tac.Tenants.Split(',').Length;
                totalAreaSum += ten.ttt.TotalArea;
                livingAreaSum += ten.ttt.LivingArea;

            }
            foreach (var ten in premiseAndSubPremiseOwner)
            {
                countInhabitant += ten.ttt.oap.Owners.Split(',').Length;
                totalAreaSum += ten.ttt.TotalArea;
                livingAreaSum += ten.ttt.LivingArea;
            }
            totalAreaSum = Math.Round(totalAreaSum, 2);
            livingAreaSum = Math.Round(livingAreaSum, 2);
            return Json(new { date, countMKD, countTenancy, countOwner, countInhabitant, totalAreaSum, livingAreaSum });
        }
    }
}