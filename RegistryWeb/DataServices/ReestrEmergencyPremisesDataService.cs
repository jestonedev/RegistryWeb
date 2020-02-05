using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryWeb.DataServices
{
    public class ReestrEmergencyPremisesDataService : ListDataService<ReestrEmergencyPremisesVM, ReestrEmergencyPremisesFilter>
    {
        private readonly IQueryable<OwnerBuildingAssoc> ownerBuildingsAssoc;
        private readonly IQueryable<OwnerPremiseAssoc> ownerPremisesAssoc;
        private readonly IQueryable<OwnerSubPremiseAssoc> ownerSubPremisesAssoc;

        private readonly IQueryable<TenancyBuildingAssoc> tenancyBuildingsAssoc;
        private readonly IQueryable<TenancyPremiseAssoc> tenancyPremisesAssoc;
        private readonly IQueryable<TenancySubPremiseAssoc> tenancySubPremisesAssoc;

        private readonly string connString;
        private readonly string activityManagerPath;

        public ReestrEmergencyPremisesDataService(RegistryContext registryContext, IConfiguration config, IHttpContextAccessor httpContextAccessor) : base(registryContext)
        {
            ownerBuildingsAssoc = registryContext.OwnerBuildingsAssoc
                .Include(oba => oba.BuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            ownerPremisesAssoc = registryContext.OwnerPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            ownerSubPremisesAssoc = registryContext.OwnerSubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();

            tenancyBuildingsAssoc = registryContext.TenancyBuildingsAssoc
                .Include(oba => oba.BuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            tenancyPremisesAssoc = registryContext.TenancyPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            tenancySubPremisesAssoc = registryContext.TenancySubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();

            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            activityManagerPath = config.GetValue<string>("ActivityManagerPath");
        }

        public override ReestrEmergencyPremisesVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, ReestrEmergencyPremisesFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }

        internal ReestrEmergencyPremisesVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            ReestrEmergencyPremisesFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var mkd = GetEmergencyMKD();
            var tenancyProcessesReestr = GetTenancyProcessesReestr(mkd);
            var onwerProcessesReestr = GetOwnerProcessesReestr(mkd);
            var reestr = tenancyProcessesReestr.Union(onwerProcessesReestr);
            viewModel.PageOptions.TotalRows = reestr.Count();
            reestr = ReestrFilter(reestr, viewModel.FilterOptions);
            var count = reestr.Count();
            viewModel.PageOptions.Rows = count;
            if (!viewModel.FilterOptions.IsEmpty())
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.Reestr = GetReestrPage(reestr, viewModel.PageOptions).ToList();
            return viewModel;
        }

        private IQueryable<ProcessOwnership> ReestrFilter(IQueryable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                reestr = ProcessOwnershipTypeFilter(reestr, filterOptions);
                reestr = PersonsFilter(reestr, filterOptions);
                reestr = AddressFilter(reestr, filterOptions);
            }
            return reestr;
        }

        private IQueryable<ProcessOwnership> ProcessOwnershipTypeFilter(IQueryable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (filterOptions.ProcessOwnershipType == ProcessOwnershipTypeEnum.All)
                return reestr;
            return reestr.Where(r => r.Type == filterOptions.ProcessOwnershipType);
        }

        private IQueryable<ProcessOwnership> PersonsFilter(IQueryable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (string.IsNullOrWhiteSpace(filterOptions.Persons))
                return reestr;
            return reestr.Where(p => p.Persons.ToLower().Contains(filterOptions.Persons.Trim().ToLower()));
        }

        private IQueryable<ProcessOwnership> AddressFilter(IQueryable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return reestr;
            if (filterOptions.ProcessOwnershipType == ProcessOwnershipTypeEnum.Private)
                return GetAddressFilter(reestr, filterOptions, ownerBuildingsAssoc, ownerPremisesAssoc, ownerSubPremisesAssoc);
            if (filterOptions.ProcessOwnershipType == ProcessOwnershipTypeEnum.Municipal)
                return GetAddressFilter(reestr, filterOptions, tenancyBuildingsAssoc, tenancyPremisesAssoc, tenancySubPremisesAssoc);
            var ownerReestr = GetAddressFilter(reestr, filterOptions, ownerBuildingsAssoc, ownerPremisesAssoc, ownerSubPremisesAssoc);
            var tenancyReestr = GetAddressFilter(reestr, filterOptions, tenancyBuildingsAssoc, tenancyPremisesAssoc, tenancySubPremisesAssoc);
            return ownerReestr.Union(tenancyReestr);
        }

        private IQueryable<ProcessOwnership> GetAddressFilter(IQueryable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions,
            IQueryable<IBuildingAssoc> buildingsAssoc, IQueryable<IPremiseAssoc> premisesAssoc, IQueryable<ISubPremiseAssoc> subPremisesAssoc)
        {
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                var idBuildingProcesses = buildingsAssoc
                    .Where(oba => oba.BuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                //query.Join(idProcesses, q => q.IdProcess, idProc => idProc, (q, idProc) => q);
                return
                    from r in reestr
                    join idProcess in idProcesses on r.Id equals idProcess
                    select r;
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return reestr;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                var idBuildingProcesses = buildingsAssoc
                    .Where(oba => oba.IdBuilding == id)
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuilding == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding == id)
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                return
                    from r in reestr
                    join idProcess in idProcesses on r.Id equals idProcess
                    select r;
            }
            if (filterOptions.Address.AddressType == AddressTypes.Premise)
            {
                var idPremiseProcesses = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdPremises == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises == id)
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idPremiseProcesses.Union(idSubPremiseProcesses);
                return
                    from r in reestr
                    join idProcess in idProcesses on r.Id equals idProcess
                    select r;
            }
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                var idProcesses = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdSubPremises == id)
                    .Select(ospa => ospa.IdProcess);
                return
                    from q in reestr
                    join idProcess in idProcesses on q.Id equals idProcess
                    select q;
            }
            return reestr;
        }

        private IQueryable<ProcessOwnership> GetReestrPage(IQueryable<ProcessOwnership> reestr, PageOptions pageOptions)
        {
            return reestr
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        internal byte[] GetFileReestr(string sqlDriver)
        {
            var logStr = new StringBuilder();
            try
            {
                var p = new Process();
                var configXml = activityManagerPath + "templates\\registry_web\\owners\\reestr.xml";
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", "reestr_" + Guid.NewGuid().ToString() + ".docx");
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = activityManagerPath + "ActivityManager.exe";
                p.StartInfo.Arguments = " config=\"" + configXml + "\" destFileName=\"" + destFileName +
                    "\" connectionString=\"Driver={" + sqlDriver + "};" +
                    connString + "\"";
                logStr.Append("<dl>\n<dt>Arguments\n<dd>" + p.StartInfo.Arguments + "\n");
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                var file = File.ReadAllBytes(destFileName);
                File.Delete(destFileName);
                return file;
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                throw new Exception(logStr.ToString());
            }
        }

        internal IQueryable<Building> GetEmergencyMKD()
        {
            var numbers = new int[] { 1, 2, 6, 7 };
            var mkd = (
                from b in registryContext.Buildings
                join ttt in (
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
                ) on b.IdBuilding equals ttt.oba.IdBuilding
                where ttt.owr.IdOwnershipRightType == 7
                select b
            );
            return mkd;
        }

        internal IQueryable<ProcessOwnership> GetTenancyProcessesReestr(IEnumerable<Building> mkd)
        {
            var tap1 =
                from tap in registryContext.TenancyActiveProcesses
                join tpa in registryContext.TenancyPremisesAssoc
                    on tap.IdProcess equals tpa.IdProcess
                join p in registryContext.Premises
                    on tpa.IdPremise equals p.IdPremises
                join premType in registryContext.PremisesTypes
                    on p.IdPremisesType equals premType.IdPremisesType
                join b in registryContext.Buildings
                    on p.IdBuilding equals b.IdBuilding
                join street in registryContext.KladrStreets
                    on b.IdStreet equals street.IdStreet
                join m in mkd
                    on b.IdBuilding equals m.IdBuilding
                group new
                {
                    tap,
                    address = street.StreetName + ", д." + b.House + ", " + premType.PremisesTypeShort + p.PremisesNum,
                    numRooms = p.NumRooms,
                    totalArea = p.TotalArea,
                    livingArea = p.LivingArea
                } by tap into procGr
                select new ProcessOwnership()
                {
                    Id = procGr.Key.IdProcess,
                    Addresses = procGr.Select(p => p.address),
                    Type = ProcessOwnershipTypeEnum.Municipal,
                    Persons = procGr.Key.Tenants,
                    NumRooms = procGr.Sum(p => p.numRooms),
                    TotalArea = procGr.Sum(p => p.totalArea),
                    LivingArea = procGr.Sum(p => p.livingArea)
                };
            var tap2 =
                from tap in registryContext.TenancyActiveProcesses
                join tspa in registryContext.TenancySubPremisesAssoc
                    on tap.IdProcess equals tspa.IdProcess
                join sp in registryContext.SubPremises
                    on tspa.IdSubPremise equals sp.IdSubPremises
                join p in registryContext.Premises
                    on sp.IdPremises equals p.IdPremises
                join premType in registryContext.PremisesTypes
                    on p.IdPremisesType equals premType.IdPremisesType
                join b in registryContext.Buildings
                    on p.IdBuilding equals b.IdBuilding
                join street in registryContext.KladrStreets
                    on b.IdStreet equals street.IdStreet
                join m in mkd
                    on b.IdBuilding equals m.IdBuilding
                group new
                {
                    tap,
                    address = street.StreetName + ", д." + b.House + ", " + premType.PremisesTypeShort + p.PremisesNum + ", ком." + sp.SubPremisesNum,
                    numRooms = 1,
                    totalArea = sp.TotalArea,
                    livingArea = sp.LivingArea
                } by tap into procGr
                select new ProcessOwnership()
                {
                    Id = procGr.Key.IdProcess,
                    Addresses = procGr.Select(sp => sp.address),
                    Type = ProcessOwnershipTypeEnum.Municipal,
                    Persons = procGr.Key.Tenants,
                    NumRooms = procGr.Sum(sp => sp.numRooms),
                    TotalArea = procGr.Sum(sp => sp.totalArea),
                    LivingArea = procGr.Sum(sp => sp.livingArea)
                };        
            return tap1.Union(tap2);
        }

        internal IQueryable<ProcessOwnership> GetOwnerProcessesReestr(IEnumerable<Building> mkd)
        {
            var oap1 =
                from oap in registryContext.OwnerActiveProcesses
                join opa in registryContext.OwnerPremisesAssoc
                    on oap.IdProcess equals opa.IdProcess
                join p in registryContext.Premises
                    on opa.IdPremise equals p.IdPremises
                join premType in registryContext.PremisesTypes
                    on p.IdPremisesType equals premType.IdPremisesType
                join b in registryContext.Buildings
                    on p.IdBuilding equals b.IdBuilding
                join street in registryContext.KladrStreets
                    on b.IdStreet equals street.IdStreet
                join m in mkd
                    on b.IdBuilding equals m.IdBuilding
                group new
                {
                    oap,
                    address = street.StreetName + ", д." + b.House + ", " + premType.PremisesTypeShort + p.PremisesNum,
                    numRooms = p.NumRooms,
                    totalArea = p.TotalArea,
                    livingArea = p.LivingArea
                } by oap into procGr
                select new ProcessOwnership()
                {
                    Id = procGr.Key.IdProcess,
                    Addresses = procGr.Select(p => p.address),
                    Type = ProcessOwnershipTypeEnum.Private,
                    Persons = procGr.Key.Owners,
                    NumRooms = procGr.Sum(p => p.numRooms),
                    TotalArea = procGr.Sum(p => p.totalArea),
                    LivingArea = procGr.Sum(p => p.livingArea)
                };
            var oap2 =
                from oap in registryContext.OwnerActiveProcesses
                join ospa in registryContext.OwnerSubPremisesAssoc
                    on oap.IdProcess equals ospa.IdProcess
                join sp in registryContext.SubPremises
                    on ospa.IdSubPremise equals sp.IdSubPremises
                join p in registryContext.Premises
                    on sp.IdPremises equals p.IdPremises
                join premType in registryContext.PremisesTypes
                    on p.IdPremisesType equals premType.IdPremisesType
                join b in registryContext.Buildings
                    on p.IdBuilding equals b.IdBuilding
                join street in registryContext.KladrStreets
                    on b.IdStreet equals street.IdStreet
                join m in mkd
                    on b.IdBuilding equals m.IdBuilding
                group new
                {
                    oap,
                    address = street.StreetName + ", д." + b.House + ", " + premType.PremisesTypeShort + p.PremisesNum + ", ком." + sp.SubPremisesNum,
                    numRooms = 1,
                    totalArea = sp.TotalArea,
                    livingArea = sp.LivingArea
                } by oap into procGr
                select new ProcessOwnership()
                {
                    Id = procGr.Key.IdProcess,
                    Addresses = procGr.Select(sp => sp.address),
                    Type = ProcessOwnershipTypeEnum.Private,
                    Persons = procGr.Key.Owners,
                    NumRooms = procGr.Sum(sp => sp.numRooms),
                    TotalArea = procGr.Sum(sp => sp.totalArea),
                    LivingArea = procGr.Sum(sp => sp.livingArea)
                };
            return oap1.Union(oap2);
        }
    }
}
